public class UniversalPlant : PlantScript
{
    // This class is a zombie class. Literally here to show the potential of inheritance. Idk if useful tbh...
    // since everything can be covered under module now, this class is kinda obselete... wouldn't hurt to keep as memory right? :3

    //public PlantModules[] plantModulesInit; //set through editor. Can't do in SO because null at compile time. 
    // Doesn't work.... screw inspector. Unity runs constructor compile before reading from editor... Set it somewhere.
    public UniversalPlant() // called automatically by Unity at compile
    {
        // load from default modules if not found in save, otherwise load from save. Dynamic!
        // can directly assign modules here, but requires a script for every plant...
        // also won't tackle dynamic since we can't check saveData due to it being null during compile
    }
}
