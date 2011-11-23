param($installPath, $toolsPath, $package, $project)
$project.Object.References.Add("System.Transactions") | Out-Null
$project.Object.References.Add("System.Data.Entity") | Out-Null
