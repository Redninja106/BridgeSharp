namespace Bridge;
public abstract record Definition(int ID, string Name)
{
    private static int nextID = 0;
    
    public static int GetNextID()
    {
        return nextID++;
    }
}