public class Bob : PlantScript
{
    public Bob() // automatically called by Unity.
    {
        productionModules.Add(new ProduceFruit(this));
        productionModules.Add(new ProduceOxygen(this));
    }
}


