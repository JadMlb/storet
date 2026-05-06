export abstract class MappingProfile
{
  public static mapObjectToString (value: any) : string
  {
    if (!value)
      return "";
      
    if (typeof value === "object")
      return JSON.stringify (
                    Object.fromEntries (
                      Object.entries (value)
                            .sort ((e1, e2) => e1[0] > e2[0] ? 1 : -1)
                    )
                  );
      
    return `${value}`;
  }
}