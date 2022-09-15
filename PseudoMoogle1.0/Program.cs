using MyMoogleEngine;

var Directorio = new MyDirectory();

var Documentos = new AllDocuments(Directorio.GetDirectories());

Documentos.TheDocuments[0].GetSplitModText();