C# Driver Version 1.4 Release Notes
===================================

The major feature of this release is support for LINQ queries. Some of the other
changes (e.g. new IBsonSerializer methods) are also in support of the new LINQ
implementation.

File by file change logs are available at:

https://github.com/mongodb/mongo-csharp-driver/blob/master/Release%20Notes/Change%20Log%20v1.4-Bson.txt
https://github.com/mongodb/mongo-csharp-driver/blob/master/Release%20Notes/Change%20Log%20v1.4-Driver.txt

These release notes describe the changes at a higher level, and omit describing
some of the minor changes.

Breaking changes
----------------

There are some breaking changes in this release. Some of them are only breaking
at the binary level and are easily taken care of by simply recompiling your
application. Others require minor changes to your source code. Many of the 
breaking changes are in low level classes, and these won't affect most users,
unless for example you are doing things like writing custom serializers.

Please read these release notes carefully before adopting the new 1.4 release
of the C# driver to determine if any of the breaking changes affect you.

LINQ query support
------------------

As stated previously, the major feature of this release is support for LINQ
queries. These release notes don't describe the new LINQ support, for that
please refer to the online LINQ tutorial at:

http://www.mongodb.org/display/DOCS/CSharp+Driver+LINQ+Tutorial

(Please note that the LINQ tutorial won't be available until several weeks 
after the 1.4 release has been shipped. Sorry.)

CLS compliance
--------------

Both the MongoDB.Bson.dll and MongoDB.Driver.dll libraries have been marked
as CLS compliant, which should make them more useful from other .NET languages.
Most of the changes required to make the libraries CLS compliant are not even
visible in the public interface.

Release builds
--------------

Starting with the 1.4 version we are shipping Release builds of the DLLs.

Code formatting changes
-----------------------

In response to popular demand the code base has been reformatted using the
default Visual Studio C# code formatting settings. We have also adopted
the convention of prefixing instance field names with a single "_" and static
fields names with a double "__" (while this convention for static fields is 
not common it is very useful). One of the nice benefits of these conventions
is that the drop down menu in Visual Studio that displays class members ends
up grouping all static fields first, followed by instance fields, followed by 
the rest of the properties and methods.

BSON library changes
====================

ArraySerializationOptions
-------------------------

This new class allows you to specify serialization options for array-like
members. Initially the only option available is to specify serialization
options for the items of the array. When using attributes to specify
serialization options any attributes that don't apply to the collection as a
whole implictly apply to the items of the collection.

BsonDateTime is now a pure BSON DateTime value
----------------------------------------------

In previous versions the BsonDateTime class stored its value twice in two
different private fields: _millisecondsSinceEpoch (a long) and _value (a .NET
DateTime). The idea was that you could store a .NET DateTime without losing any
precision. However, that turns out to be confusing because as soon as you save
the BsonDateTime value to the database and read it back you are going to lose
precision anyway, so you might as well lose it right up front and not make 
false promises.

BsonDateTime also has two new helper methods: ToLocalTime and ToUniversalTime.
These methods convert the BSON DateTime to either local or UTC .NET DateTime
values. There are also new AsLocalTime and AsUniversalTime properties in
BsonValues that can be used to convert BsonValues to .NET DateTime values (like
all AsXyz properties in BsonValue they throw an InvalidCastException if the
BsonValue is not actually a BsonDateTime).

BsonIgnoreExtraElements attribute
---------------------------------

The BsonIgnoreExtraElements attribute has a new property called Inherited. If
this property is set to true then all classes derived from this one will
automatically inherit this setting, which makes it easy to set it for an
entire class hierarchy at once.

BsonIgnoreIfDefault attribute
-----------------------------

This new attribute allows you to specify that you want a field to be ignored
during serialization if it is equal to the default value. This replaces the
SerializeDefaultValue parameter of the BsonDefaultValue attribute. By making
this a separate attribute you can specify that you want the default value
ignored without having to specify the default value as well.

BsonReader: CurrentBsonType vs GetCurrentBsonType
-------------------------------------------------

In previous versions the CurrentBsonType property had side effects. In general
it is bad form for the get accessor of a property to have side effects, as even
something as simple as looking at the value of the property in a debugger can
have unintended consequences. Therefore, in the 1.4 release the CurrentBsonType
property has no side effects. The previous behavior is now implemented in the
GetCurrentBsonType method. While this is mostly an internal change, *if* you
have written a custom serializer that used the CurrentBsonType property and
were relying on its side effects you will have to change your custom serializer
to call GetCurrentBsonType instead.

ConventionProfile new conventions
---------------------------------

The ConventionProfile class has two new conventions: IgnoreIfDefaultConvention
and SerializationOptionsConvention. Also, the SerializeDefaultValueConvention
has been obsoleted in favor of the new IgnoreIfDefaultConvention.

DictionarySerializationOptions
------------------------------

This class has a new property called ItemSerializationOptions that can be used
to specify the options to use when serializing the value of the items in the
dictionary. When using attributes to specify serialization options, any
attributes that don't apply to the dictionary as a whole implicitly apply to
the value of the items in the dictionary.

ExtraElements
-------------

Previous versions of the C# driver allowed you to specify a field of the class
to be used to store any extra elements encountered during deserialization.
However, this field *had* to be of type BsonDocument, which meant introducing
a dependency on the driver into your data model classes (which some developers
don't want to do). You now have the additional option of declaring your
ExtraElements field to be of type IDictionary\<string, object\> instead.

IBsonSerializationOptions
-------------------------

The IBsonSerializationOptions has several new methods. ApplyAttribute is used
during the AutoMap process to apply an attribute to some serialization options
that are being built incrementally (starting from the default serialization
options). This provides an extensible mechanism for applying new attributes to
new serialization options classes. The Clone and Freeze methods are introduced
to allow serialization options to be converted to read-only once initialization
is complete to provide thread safety.

IBsonSerializer
---------------

The IBsonSerializer has several new methods. GetDefaultSerializationOptions
provides an initial set of serialization options that any serialization 
attributes found can be applied against. GetItemSerializationInfo provides
serialization info about the items and applies only to serializers for 
collection-like classes. GetMemberSerializationInfo provides serialization 
info about members of a class. The last two are used in the implementation 
of LINQ queries.

Image/Bitmap serializers
------------------------

New serializers have been provided for the Image abstract base class and the
Bitmap class derived from it.

ISupportInitialize
------------------

The ISupportInitialize interface defines two methods: BeginInit and EndInit.
The BsonClassMapSerializer now checks whether the class being deserialized
implements this interface, and if so, calls BeginInit just before it starts
to deserialize a class, and EndInit just after it has finished. You can 
use this feature to do any pre- or post-processing.

ObjectId/BsonObjectId creation
------------------------------

ObjectId (and BsonObjectId) have a new constructor that allows you to supply
the timestamp as a .NET DateTime value and it will automatically be converted
to seconds since the Unix Epoch. These new constructors are useful if you want
to create artificial ObjectIds to use in range based ObjectId queries (in
which case you will usually set the machine, pid and increment fields to zero).

There are also two new overloads of GenerateNewId that allow you to provide
the desired timestamp as either an int or a .NET DateTime. These new overloads
are useful if you need to create backdated ObjectIds. When generating backdated
ObjectIds there is a slight risk that you might create an ObjectId that is
not unique (but that risk is very small).

TimeSpanSerializationOptions
----------------------------

You can now choose any of the following representations for a TimeSpan: string,
double, Int32 or Int64. In addition, when using any of the numeric
representations, you can use the Units property to choose the units that the
numeric value is in (choose from: Ticks, Days, Hours, Minutes, Seconds, 
Milliseconds and Nanoseconds).

Driver changes
==============

Authentication support improved
-------------------------------

Operations that require admin credentials previously required you to set the
DefaultCredentials of MongoServerSetttings to admin credentials. But that is
undesirable because it provides the client code full access to all databases, 
essentially negating the benefit of using authentication in the first place.
In the 1.4 release all operations that require admin credentials have a new
overload where you can provide the needed credentials; you no longer have to
set the DefaultCredentials. Another option is to store credentials for the 
admin database in the new MongoCredentialsStore.

Connection pool defaults changed
--------------------------------

The default value of WaitQueueMultiple has been changed from 1.0 to 5.0 and the
default value of WaitQueueTimeout has been changed from 0.5 seconds to 2
minutes. These new values are taken from the Java driver, where they have
reportedly been working well for users running under heavy loads. These new
values mean that many more threads can be waiting for a longer time before a 
timeout exception is thrown.

Exceptions are no longer caught and rethrown when possible
----------------------------------------------------------

Wherever possible exception handling code that used to use catch exceptions
and rethrow them after processing them has been changed to roughly equivalent
code that uses try/finally to accomplish the same objective. This is specially
helpful if you are running the debugger set to stop whenever an exception is
thrown.

IBsonSerializable semi-deprecated
---------------------------------

The LINQ support relies heavily on the new methods added to IBsonSerializer.
Because of this it is highly encouraged that *if* you have to handle your own
serialization that you always opt to write an IBsonSerializer for your class 
instead of having it implement IBsonSerializable (see the notes for MongoDBRef 
and SystemProfileInfo for examples of where the driver itself has switched 
from IBsonSerializable to using a IBsonSerializer). IBsonSerializable still 
has a modest role to play in classes that just need to be serialized quickly 
and simply and for which we won't be writing LINQ queries (for example, the 
driver's Builders and Wrappers still use IBsonSerializable).

LINQ query support
------------------

As mentioned earlier in the release notes more information about the new
support for LINQ queries can be found in the forthcoming LINQ tutorial:

http://www.mongodb.org/display/DOCS/CSharp+Driver+LINQ+Tutorial

Locking issues
--------------

A number of locking issues related to connection pooling have been resolved.
These issues were particularly likely to occur if you had more threads than
the maximum size of the connection pool and were using the connections heavily
enough that the connection pool could be used up.

MongoCredentialsStore
---------------------

You can now create a credentials store which contains credentials for multiple
databases (the name of the database is the key and the credentials are the
value). The credentials store must be set up (in the MongoServerSettings) 
before you call MongoServer.Create, so it is only intended for scenarios 
where you have a fixed set of credentials that aren't going to change at runtime.

MongoDBRef no longer implements IBsonSerializable
-------------------------------------------------

MongoDBRef used to handle its own serialization by virtue of implementing
IBsonSerializable. But the IBsonSerializable interface is not helpful when we
try to add support for writing LINQ queries against components of a MongoDBRef.
Instead, there is now a MongoDBRefSerializer which handles serialization of
MongoDBRefs, as well as implementing GetMemberSerializationInfo which enables 
the LINQ implementation to support LINQ queries against MongoDBRefs.

MongoInsertOptions/MongoUpdateOptions constructor changed
---------------------------------------------------------

The constructors for MongoInsertOptions and MongoUpdateOptions used to require
that the collection be passed in as a parameter. The purpose was to allow
the constructor to inherit some of the options from the collection settings.
To the developer however, this was awkward, as it required providing the
collection where it seemed to be redundant. By handling default values in a
different way we no longer require the collection to be supplied to the 
constructors. The old constructors (that require the collection parameter) are
still temporarily supported but have been marked as deprecated with a warning.

MongoServer Admin properties and methods removed
------------------------------------------------

The following Admin properties and methods have been removed from MongoServer:
AdminDatabase, GetAdminDatabase, RunAdminCommand, and RunAdminCommandAs. The
reason for removing them is that many developers would never use them anyway,
and adding new overloads for providing admin credentials would have resulted
in even more of these rarely used properties and methods. If you were using
any of these methods or properties they can easily be replaced with calls to 
methods of an instance of MongoDatabase (use one of the overloads of 
GetDatabase with "admin" as the database name to get a reference to the admin
database).

RequestStart/RequestDone
------------------------

Recall that the purpose of RequestStart is to tell the driver that a series of
operations should all be done using the same connection (which in the case of a
replica set also implies using the same member of the connection set). Which
member of a replica set was chosen depended on the slaveOk parameter: a value 
of false meant that the primary had to be used, and a value of true meant that
an arbitrary secondary could be used. A new overload of RequestStart now allows
the caller to specify which member should be used, which can be very useful for
implementing custom query routing algorithms or for querying specific members
of a replica set. In general though, keep in mind that you should *not* be
using RequestStart unless you have an unusual scenario which requires it.

SocketTimeout default changed
-----------------------------

The default value for SocketTimeout has been changed from 30 seconds to 0,
which is a special value meaning to use the operating system default value,
which in turn is infinity. If you actually want a SocketTimeout you now
have to set it yourself. The SocketTimeout is currently a server level setting, 
but most likely in a future release it will be possible to set it at other
levels, including for individual operations.

SystemProfileInfo no longer implements IBsonSerializable
--------------------------------------------------------

See the notes for MongoDBRef. SystemProfileInfo no longer implements 
IBsonSerializable for the same reasons, and there is a new 
SystemProfileInfoSerializer instead.
