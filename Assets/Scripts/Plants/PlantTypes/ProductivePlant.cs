public class ProductivePlant : PlantScript
{
    public ProductivePlant() // automatically called by Unity.
    {
        productionModules.Add(new ProduceFruit(this));
        productionModules.Add(new ProduceOxygen(this));
    }
}


