(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
# Pfim

Pfim is an incredibly simple and fast image decoding library with an emphasis
on being backend and frontend agnostic.

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Pfim library can be <a href="https://nuget.org/packages/Pfim">installed from NuGet</a>:
      <pre>PM> Install-Package Pfim</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

## Getting Started

    [lang=csharp]
    // Load image from file path
    IImage image = Pfim.FromFile(@"C:\image.tga");

## Samples & Documentation

 * [Tutorial](tutorial.html) contains a walkthrough some of the API for more uses.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
## Contributing and copyright

The project is hosted on [GitHub][gh] where you can [report issues][issues],
fork the project and submit pull requests. If you're adding a new public API,
please also  consider adding [samples][content] that can be turned into a
documentation. You might also want to read the [library design notes][readme]
to understand how it works.

The library is available under the MIT license, which allows modification and
redistribution for both commercial and non-commercial purposes. For more
information see the [License file][license].

  [content]: https://github.com/nickbabcock/Pfim/tree/master/docs/content
  [gh]: https://github.com/nickbabcock/Pfim
  [issues]: https://github.com/nickbabcock/Pfim/issues
  [readme]: https://github.com/nickbabcock/Pfim/blob/master/README.md
  [license]: https://github.com/nickbabcock/Pfim/blob/master/LICENSE.txt
*)
