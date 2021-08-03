using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NzbDrone.Common.Expansive
{
    public static class Expansive
    {
        private static PatternStyle _patternStyle;

        public static bool RequireAllExpansions { get; set; }

        public static Func<string, string> DefaultExpansionFactory { get; set; }

        static Expansive()
        {
            Initialize();
        }

        public static string Expand(this string source)
        {
            return source.Expand(DefaultExpansionFactory);
        }

        public static string Expand(this string source, params string[] args)
        {
            var output = source;
            var tokens = new List<string>();
            var pattern = new Regex(_patternStyle.TokenMatchPattern, RegexOptions.IgnoreCase);
            var calls = new Stack<string>();
            string callingToken = null;

            while (pattern.IsMatch(output))
            {
                foreach (Match match in pattern.Matches(output))
                {
                    var token = _patternStyle.TokenReplaceFilter(match.Value);
                    var tokenIndex = 0;
                    if (!tokens.Contains(token))
                    {
                        tokens.Add(token);
                        tokenIndex = tokens.Count - 1;
                    }
                    else
                    {
                        tokenIndex = tokens.IndexOf(token);
                    }

                    output = Regex.Replace(output, _patternStyle.OutputFilter(match.Value), "{" + tokenIndex + "}");
                }
            }

            var newArgs = new List<string>();
            foreach (var arg in args)
            {
                var newArg = arg;
                var tokenPattern = new Regex(_patternStyle.TokenFilter(string.Join("|", tokens)));
                while (tokenPattern.IsMatch(newArg))
                {
                    foreach (Match match in tokenPattern.Matches(newArg))
                    {
                        var token = _patternStyle.TokenReplaceFilter(match.Value);
                        if (calls.Contains(string.Format("{0}:{1}", callingToken, token)))
                        {
                            throw new CircularReferenceException(string.Format("Circular Reference Detected for token '{0}'.", callingToken));
                        }

                        calls.Push(string.Format("{0}:{1}", callingToken, token));
                        callingToken = token;
                        newArg = Regex.Replace(newArg, _patternStyle.OutputFilter(match.Value), args[tokens.IndexOf(token)]);
                    }
                }

                newArgs.Add(newArg);
            }

            return string.Format(output, newArgs.ToArray());
        }

        public static string Expand(this string source, Func<string, string> expansionFactory)
        {
            return source.ExpandInternal(expansionFactory);
        }

        public static string Expand(this string source, object model)
        {
            return source.ExpandInternal(
                name =>
                {
                    IDictionary<string, object> modelDict = model.ToDictionary();
                    if (RequireAllExpansions && !modelDict.ContainsKey(name))
                    {
                        return "";
                    }

                    if (modelDict[name] == null)
                    {
                        return "";
                    }

                    return modelDict[name].ToString();
                });
        }

        private static void Initialize()
        {
            _patternStyle = new PatternStyle
                {
                    TokenMatchPattern = @"\{[a-zA-Z]\w*\}",
                    TokenReplaceFilter = token => token.Replace("{", "").Replace("}", ""),
                    OutputFilter = output => (output.StartsWith("{") && output.EndsWith("}") ? output : @"\{" + output + @"\}"),
                    TokenFilter = tokens => "{(" + tokens + ")}"
                };
        }

        private static string ExpandInternal(this string source, Func<string, string> expansionFactory)
        {
            if (expansionFactory == null)
            {
                throw new ApplicationException("ExpansionFactory not defined.\nDefine a DefaultExpansionFactory or call Expand(source, Func<string, string> expansionFactory))");
            }

            var pattern = new Regex(_patternStyle.TokenMatchPattern, RegexOptions.IgnoreCase);

            var callTreeParent = new Tree<string>("root").Root;

            return source.Explode(pattern, _patternStyle, expansionFactory, callTreeParent);
        }

        private static string Explode(this string source, Regex pattern, PatternStyle patternStyle, Func<string, string> expansionFactory, TreeNode<string> parent)
        {
            var output = source;
            while (output.HasChildren(pattern))
            {
                foreach (Match match in pattern.Matches(source))
                {
                    var child = match.Value;
                    var token = patternStyle.TokenReplaceFilter(match.Value);

                    var thisNode = parent.Children.Add(token);

                    // if we have already encountered this token in this call tree, we have a circular reference
                    if (thisNode.CallTree.Contains(token))
                    {
                        throw new CircularReferenceException(string.Format("Circular Reference Detected for token '{0}'. Call Tree: {1}->{2}",
                                                                           token,
                                                                           string.Join("->", thisNode.CallTree.ToArray().Reverse()),
                                                                           token));
                    }

                    // expand this match
                    var expandedValue = expansionFactory(token);

                    // Replace the match with the expanded value
                    child = Regex.Replace(child, patternStyle.OutputFilter(match.Value), expandedValue);

                    // Recursively expand the child until we no longer encounter nested tokens (or hit a circular reference)
                    child = child.Explode(pattern, patternStyle, expansionFactory, thisNode);

                    // finally, replace the match in the output with the fully-expanded value
                    output = Regex.Replace(output, patternStyle.OutputFilter(match.Value), child);
                }
            }

            return output;
        }

        private static bool HasChildren(this string token, Regex pattern)
        {
            return pattern.IsMatch(token);
        }

        /// <summary>
        /// Turns the object into an ExpandoObject
        /// </summary>
        private static dynamic ToExpando(this object o)
        {
            var result = new ExpandoObject();
            var d = result as IDictionary<string, object>; //work with the Expando as a Dictionary
            if (o is ExpandoObject)
            {
                return o; //shouldn't have to... but just in case
            }

            if (o is NameValueCollection || o.GetType().IsSubclassOf(typeof(NameValueCollection)))
            {
                var nv = (NameValueCollection)o;
                nv.Cast<string>().Select(key => new KeyValuePair<string, object>(key, nv[key])).ToList().ForEach(i => d.Add(i));
            }
            else
            {
                var props = o.GetType().GetProperties();
                foreach (var item in props)
                {
                    d.Add(item.Name, item.GetValue(o, null));
                }
            }

            return result;
        }

        /// <summary>
        /// Turns the object into a Dictionary
        /// </summary>
        private static IDictionary<string, object> ToDictionary(this object thingy)
        {
            return (IDictionary<string, object>)thingy.ToExpando();
        }
    }
}
