# multithreading
A sample that illustrates how the multi-threaded code should work

The ideas behind the implementatioon:

1. THe UI is made as simple as possible (I am not an expert in Usability and UX);
2. Tried WPF to handle various DPI screen settings (checked on 15" FHD/100% and on 13" QHD+/150%)
3. The tree-walking is implemented in the ABR fasion to be able to writeto the XML file via XML writer (low memory consumption)
4. For some cases is seems like .Net does not provide good methods to check security (long paths); so this is not supported and I decided to leave this out of scope of this implementation, but there is a technical vision of solving that.
5. Some parts are covered with unit tests; MSTest is used as an OOTB framework shipped with MS VS. There are better options but MSTEst remains as the one that does not add extra 3rd-party DLLs.
6. The thread that updates UI in fact is not needed and is added more to meet the task constraints. I would get rid of this thread.
