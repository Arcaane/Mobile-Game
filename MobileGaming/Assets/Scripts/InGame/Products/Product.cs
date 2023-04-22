using System;

[Serializable]
public class Product
{
    public ProductData data;

    public Product(ProductData data)
    {
        this.data = data;
    }

    public Product(ProductColor color, ProductShape shape)
    {
        data.Color = color;
        data.Shape = shape;
    }
    
    public override string ToString()
    {
        return $"{data.Color} and {data.Shape} Product";
    }
}

[Serializable]
public struct ProductData
{
    public ProductColor Color;
    public ProductShape Shape;

    public static bool operator ==(ProductData data1, ProductData data2) 
    {
        return data1.Equals(data2);
    }

    public static bool operator !=(ProductData data1, ProductData data2) 
    {
        return !data1.Equals(data2);
    }

    public static ProductData Random => new ProductData()
    {
        Shape = GetRandomEnum<ProductShape>(),
        Color = GetRandomEnum<ProductColor>(),
    };

    public static T GetRandomEnum<T>()
    {
        var values = Enum.GetValues(typeof(T)); 
        return (T) values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}

public enum ProductShape {Hearth, Moon, Cross}
public enum ProductColor {Transparent, Red, Blue, Green}


