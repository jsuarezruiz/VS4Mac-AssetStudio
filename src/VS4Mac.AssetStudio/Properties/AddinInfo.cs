using System;
using System.Runtime.InteropServices;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "AssetStudio",
    Namespace = "MonoDevelop",
    Version = "0.1", 
    Category = "IDE extensions"
)]

[assembly: AddinName("Asset Studio")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("VS4Mac addin with functionality related to assets management.")]
[assembly: AddinAuthor("Javier Suárez Ruiz")]
[assembly: AddinUrl("https://github.com/jsuarezruiz/VS4Mac-AssetStudio")]

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]