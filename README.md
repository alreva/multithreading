# multithreading
A sample that illustrates how the multi-threaded code should work

The ideas behind the implementatioon:

1. THe UI is made as simple as possible (I am not an expert in Usability and UX);
2. There is a requirement that the directory walking results should be saved to the file; in the same time these results need to be displayed in the UI, in a tree view; I decided to display only the subset of the information in the tree view (basically, the names and the sizes);
3. Tried WPF to handle various DPI screen settings (checked on 15" FHD/100% and on 13" QHD+/150%);
4. The tree-walking is implemented in the ABR flavour to be able to writeto the XML file via XML writer (low memory consumption); this is not as good as the RAB flavour for displaying results in the UI (results show up later for the top levels);
5. For some cases is seems like .Net does not provide good methods to check security (long paths); so this is not supported and I decided to leave this out of scope of this implementation, but there is a technical vision of solving that;
6. Some parts are covered with unit tests; MSTest is used as an OOTB framework shipped with MS VS. There are better options but MSTEst remains as the one that does not add extra 3rd-party DLLs;
7. The thread that updates UI in fact is not needed and is added more to meet the task constraints. I would get rid of this thread.
