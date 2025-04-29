// Structure to hold persistence data
public struct PersistencePoint
{
    public float birth;
    public float death;

    public PersistencePoint(float birth, float persistence)
    {
        this.birth = birth;
        this.death = birth + persistence; // Death = Birth + Persistence
    }
}
