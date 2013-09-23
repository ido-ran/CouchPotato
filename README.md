CouchPotato
===========

CouchDB Object Document Mapper - EntityFramework for CouchDB.

Why
===
I've wrote Couch Potato because I've decide to replace SQL Server with CouchDB to get the agility and speed that comes with CouchDB.

How it works
============
Couch Potato maps C# POCOs to CouchDB document using a convention, which is currently not configrable but will be.
This allows this library to be able to load documents from CouchDB into objects in C#, read the objects, manipulate them 
and write the objets back to CouchDB.
The POCOs do not have special properties.
CouchDB _id and _rev fields are handled by the library.

What do I get?
==============
The key feature of this library is the ability to fetch related documents easily and efficiently.

What is the status?
===================
This is a young project. It is been used in an internet facing application but the API is likely to change.

License
=======
This project is licensed under MIT.
