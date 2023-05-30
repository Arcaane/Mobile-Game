using System;
using UnityEngine.UI;

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
        return $"{data.Color}, {data.Shape} and {data.Topping} Product";
    }

    
}

[Serializable]
public struct ProductData
{
    public ProductColor Color;
    public ProductShape Shape;
    public ProductTopping Topping;

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
        Topping = GetRandomEnum<ProductTopping>(),
    };

    public static T GetRandomEnum<T>()
    {
        var values = Enum.GetValues(typeof(T)); 
        return (T) values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
    
    public void ApplySpriteIndexes(Image shapeImage,Image contentImage,Image topingImage)
    {
        var shapeSpriteIndex = 0;
        var contentSpriteIndex = 0;
        contentImage.color = UnityEngine.Color.white;
        shapeImage.color = UnityEngine.Color.white;
        topingImage.color = Topping != ProductTopping.None ? UnityEngine.Color.clear : UnityEngine.Color.white;
        switch (Shape)
        {
            case ProductShape.Heart:
                shapeSpriteIndex = 0;
                switch (Color)
                {
                    case ProductColor.Transparent:
                        contentImage.color = UnityEngine.Color.clear;
                        break;
                    case ProductColor.Blue:
                        contentSpriteIndex = 0;
                        break;
                    case ProductColor.Green:
                        contentSpriteIndex = 1;
                        break;
                    case ProductColor.Red:
                        contentSpriteIndex = 2;
                        break;
                    default:
                        contentSpriteIndex = contentSpriteIndex;
                        break;
                }
                break;
            
            case ProductShape.Cross: 
                shapeSpriteIndex = 1;
                switch (Color)
                {
                    case ProductColor.Transparent:
                        contentImage.color = UnityEngine.Color.clear;
                        break;
                    case ProductColor.Blue:
                        contentSpriteIndex = 3;
                        break;
                    case ProductColor.Green:
                        contentSpriteIndex = 4;
                        break;
                    case ProductColor.Red:
                        contentSpriteIndex = 5;
                        break;
                    default:
                        contentSpriteIndex = contentSpriteIndex;
                        break;
                }
                break;
            
            case ProductShape.Moon:
                shapeSpriteIndex = 2;
                switch (Color)
                {
                    case ProductColor.Transparent:
                        contentImage.color = UnityEngine.Color.clear;
                        break;
                    case ProductColor.Blue:
                        contentSpriteIndex = 6;
                        break;
                    case ProductColor.Green:
                        contentSpriteIndex = 7;
                        break;
                    case ProductColor.Red:
                        contentSpriteIndex = 8;
                        break;
                    default:
                        contentSpriteIndex = contentSpriteIndex;
                        break;
                }
                break;
            
            case ProductShape.Base:
                shapeSpriteIndex = 3;
                switch (Color)
                {
                    case ProductColor.Transparent:
                        contentImage.color = UnityEngine.Color.clear;
                        break;
                    case ProductColor.Blue:
                        contentSpriteIndex = 9;
                        break;
                    case ProductColor.Green:
                        contentSpriteIndex = 10;
                        break;
                    case ProductColor.Red:
                        contentSpriteIndex = 11;
                        break;
                    default:
                        contentSpriteIndex = contentSpriteIndex;
                        break;
                }
                break;
        }

        shapeImage.sprite = ScriptableSettings.GlobalSettings.bottleShapesSprites[shapeSpriteIndex];
        contentImage.sprite = ScriptableSettings.GlobalSettings.bottleContentSprites[contentSpriteIndex];
    }
}

[Flags] public enum ProductShape {Base = 1,Heart = 2, Moon = 4, Cross = 8}
[Flags] public enum ProductColor {Transparent = 1, Red = 2, Blue = 4, Green = 8}
[Flags] public enum ProductTopping {None = 1, Yes = 2}
