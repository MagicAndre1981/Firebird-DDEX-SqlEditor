# Using ADO.NET edmx with .NET

As of this writing Visual Studio does not suport edmx wizards within .NET/.NET Core projects.<br>This obstacle is easily overcome and is probably a blessing in disguise because it forces you to take a more structured approach to setting up your data access by having your datasets and models in a separate project.

### Setup procedure

* In your solution create a standard .NetFramework Windows project and set up your ADO.NET edmx's (and preferably even your xsd's) as described in [ADO.NET provider](ado-net.md).</br>
Name your project something appropriate, like `DataAccessHub`, because this is where you'll be configuring all your data access entities. We will use the name `DataAccessHub` whenever referring to this project.
* For edmx you should complete the linking of your `DataAccessHub` models to your .NET projects before adding any objects.
* Go to each of your .NET \[Core\] projects and add links to the required edmx's, as well as distinct links to their associate tt files (eg. MyEdmxModel.tt and MyEdmxModel.Context.tt), located in `DataAccessHub`.</br>
__Warning__ Any previously existing data entity objects will have their namespaces prefixed with something like `MyDotNetApp....DataAccessHub`. You will have to go to each of these .cs classes, located within `MyEdmxModel.tt`, and remove this prefix.
* Once you have added all links and fixed any corrupted namespaces you can continue configuring your models within `DataAccessHub` as and when required.
* Any modifications or additions you make to your models will now automatically be updated in your client projects.
* You can also link your data access entities to the main form in `DataAccessHub` if you need to perform any testing on them.