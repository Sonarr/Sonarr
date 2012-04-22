C# Driver Version 1.4.1 Release Notes
=====================================

This minor release fixes a few issues found by users of the LINQ support added
in v1.4 of the C# driver and also adds support for a few new LINQ query
operators and where clauses.

File by file change logs are available at:

https://github.com/mongodb/mongo-csharp-driver/blob/master/Release%20Notes/Change%20Log%20v1.4.1-Bson.txt
https://github.com/mongodb/mongo-csharp-driver/blob/master/Release%20Notes/Change%20Log%20v1.4.1-Driver.txt

These release notes describe the changes at a higher level, and omit describing
some of the minor changes.

Breaking changes
----------------

There are no breaking changes in this release.

JIRA issues resolved
--------------------

The full list of JIRA issues resolved in this release is available at:

https://jira.mongodb.org/secure/IssueNavigator.jspa?mode=hide&requestId=11397

LINQ query support
==================

The main purpose of this minor release is to fix some issues found by users of
the new LINQ support added in v1.4.

One bug that many have encountered is a NullReferenceException when writing a
query against an inherited property.

https://jira.mongodb.org/browse/CSHARP-418

You would hit this error if you had any queries that were similar to this:

    public class B
	{
		public ObjectId Id;	
	}

	public class C : B
	{
		public int X;
	}

	var query =
	    from c in collection.AsQueryable<C>()
		where c.Id = id // class C inherits Id from class B
		select c;

Another bug that a few users have encountered is an ArgumentOutOfRangeException
when writing a LINQ query that consists of a bare AsQueryable and nothing else:

https://jira.mongodb.org/browse/CSHARP-419

as in this sample:

    var query = collection.AsQueryable<C>(); // no where clause

Normally a query would contain something else besides the call to AsQueryable
(like a where clause), but this is a legal query and is now supported.

BSON library changes
====================

MaxSerializationDepth
---------------------

The BSON serialization mechanism does not support circular references in your
object graph. In earlier versions of the C# driver if you attempted to
serialize an object with circular references you would get a
StackOverflowExpection. The 1.4.1 version now tracks the serialization depth
as it serializes an object and if it exceeds MaxSerializationDepth a
BsonSerializationException is thrown. The problem with StackOverflowException
was that it was fatal to your process, but the BsonSerializationException can
be caught and your process can continue executing if you choose.

The default MaxSerializationDepth is 100.

Interpretation of C# null vs BsonNull.Value
-------------------------------------------

When working with the BsonDocument object model a C# null is usually ignored,
specially when creating BsonDocuments using functional construction. However,
when mapping between .NET types and the BsonDocument object model a C# null
will now be mapped to a BsonNull. For example:

    var dictionary = new Dictionary<string, object> { { "x", null } };
	var document = new BsonDocument(dictionary);
	// document["x"] == BsonNull.Value

and when mapping in the reverse direction a BsonNull will map to a C# null:

	var document = new BsonDocument { { "x", BsonNull.Value } };
    var dictionary = document.ToDictionary();
	// dictionary["x"] == null

Usually mapping between .NET types and the BsonDocument object model happens
automatically as needed, but if you want to invoke the mapping yourself you
can access the BsonTypeMapper directly:

    var dictionary = new Dictionary<string, object> { { "x", null } };
	var document = BsonTypeMapper.MapToBsonValue(dictionary);
	// document["x"] == BsonNull.Value

or in the other direction:

    var document = new BsonDocument { { "x", BsonNull.Value } };
	var dictionary = (IDictionary<string, object>)BsonTypeMapper.MapToDotNetValue(document);
	// dictionary["x"] == null

Serializing read-only properties
--------------------------------

The class map based serialization support normally serializes only public
read-write properties (or fields). Sometimes it can be useful to serialize
read-only properties as well, specially if you want to query against them.
You can now opt-in your read-only properties so that they appear in the
serialized document. For example:

    public class Book
	{
	    public ObjectId Id;
		public string Title;
		[BsonElement] // opt-in the read-only LowercaseTitle property
		public string LowercaseTitle { get { return Title.ToLower(); } }
	}

Now when a Book is serialized the document will look like:

    {
	    _id : ObjectId("4f8d771dae879111d289dbc0"),
		Title : "For Whom the Bell Tolls",
		LowercaseTitle : "for whom the bell tolls"
	}

During deserialization any elements in the serialized document that
correspond to read-only properties are ignored.

Driver changes
==============

MongoServer
-----------

There is a new method called IsDatabaseNameValid that you can call to test if
a database name is valid.

MongoDatabase
-------------

There is a new method called IsCollectionNameValid that you can call to test if a
collection name is valid.

MongoGridFS
-----------

You can now disable computing the MD5 at the server when uploading a GridFS
file. You can also disable the client side verification of the MD5 that is
normally done on Upload or Download. The reason you might choose to disable
MD5 verification is that it is computationally expensive to compute the MD5.

LINQ OfType\<T\> query operator
-------------------------------

You can now use the OfType\<T\> query operator in LINQ queries. For example:

    var query = collection.AsQueryable<B>().OfType<C>();

this generates a query against the "_t" discriminator value that is used to
identify the actual type of a serialized document.

Additional expressions supported in LINQ where clauses
------------------------------------------------------

The following expressions are now supported in LINQ where clauses:

    // d is the document
    // p is a property of the document
    // c is a character constant
    // ca is an array of character constants
    // s is a string constant
    // i, j, k, n are integer constants

    where d.p.Equals(constant)
	where string.IsNullOrEmpty(d.p)
	where d.p.IndexOf(c) == i
	where d.p.IndexOf(c, j) == i
	where d.p.IndexOf(c, j, k) == i
	where d.p.IndexOf(s) == i
	where d.p.IndexOf(s, j) == i
	where d.p.IndexOf(s, j, k) == i
	where d.p.IndexOfAny(ca) == i
	where d.p.IndexOfAny(ca, j) == i
	where d.p.IndexOfAny(ca, j, k) == i
	where d.p[i] == c
	where d.p.Length == n
	where d.p.ToLower().Contains("xyz")
	where d.p.ToLower().StartsWith("xyz")
	where d.p.ToLower().EndsWith("xyz")
	where d.p.ToUpper().Contains("xyz")
	where d.p.ToUpper().StartsWith("xyz")
	where d.p.ToUpper().EndsWith("xyz")
	where d.p.Trim().Contains("xyz")
	where d.p.Trim().StartsWith("xyz")
	where d.p.Trim().EndsWith("xyz")
	where d.p.TrimStart().Contains("xyz")
	where d.p.TrimStart().StartsWith("xyz")
	where d.p.TrimStart().EndsWith("xyz")
	where d.p.TrimEnd().Contains("xyz")
	where d.p.TrimEnd().StartsWith("xyz")
	where d.p.TrimEnd().EndsWith("xyz")
	where d.GetType() == typeof(T)
	where d is T

	// you can use any combination of ToLower/ToUpper/Trim/TrimStart/TrimEnd
	// before Contains/StartsWith/EndsWith

In the 1.4 version of the C# driver the constant always had to appear on the
right of a comparison operator. That restriction is lifted in 1.4.1 so now the
following are equivalent:

    where d.Height < 60
	where 60 > d.Height

Type of \<T\> in AsQueryable can now be deduced
-----------------------------------------------

The type of \<T\> in the call to AsQueryable can now be deduced from the collection argument:

    var collection = database.GetCollection<MyDocument>("mydocuments")
	var query = collection.AsQueryable(); // <T> is deduced to be MyDocument
