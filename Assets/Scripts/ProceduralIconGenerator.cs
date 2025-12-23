// ProceduralIconGenerator.cs - UPDATED VERSION
// Fixed to use new ArmorType enum (Helmet, Chestplate, Leggings, Gloves, Boots, Shield)

using UnityEngine;

public class ProceduralIconGenerator : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private int iconSize = 128;
    [SerializeField] private bool useGradients = true;
    [SerializeField] private bool addBorder = true;
    [SerializeField] private int borderWidth = 4;
    
    // Generate icon for weapon
    public Sprite GenerateWeaponIcon(Weapon weapon)
    {
        Texture2D texture = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color backgroundColor = GetRarityColor(weapon.rarity);
        Color accentColor = GetWeaponTypeColor(weapon.weaponType);
        
        FillTexture(texture, new Color(0.1f, 0.1f, 0.1f, 1f));
        
        if (addBorder)
        {
            DrawBorder(texture, backgroundColor, borderWidth);
        }
        
        DrawWeaponShape(texture, weapon.weaponType, accentColor, backgroundColor);
        
        if (useGradients)
        {
            ApplyGradient(texture, backgroundColor);
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
    }
    
    // Generate icon for armor
    public Sprite GenerateArmorIcon(Armor armor)
    {
        Texture2D texture = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color backgroundColor = GetRarityColor(armor.rarity);
        Color accentColor = GetArmorTypeColor(armor.armorType);
        
        FillTexture(texture, new Color(0.1f, 0.1f, 0.1f, 1f));
        
        if (addBorder)
        {
            DrawBorder(texture, backgroundColor, borderWidth);
        }
        
        DrawArmorShape(texture, armor.armorType, accentColor, backgroundColor);
        
        if (useGradients)
        {
            ApplyGradient(texture, backgroundColor);
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
    }
    
    // Generate icon for collectible
    public Sprite GenerateCollectibleIcon(Collectible collectible)
    {
        Texture2D texture = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color rarityColor = GetRarityColor(collectible.rarity);
        
        FillTexture(texture, new Color(0.1f, 0.1f, 0.1f, 1f));
        
        if (addBorder)
        {
            DrawBorder(texture, rarityColor, borderWidth);
        }
        
        DrawCollectibleShape(texture, rarityColor);
        
        if (useGradients)
        {
            ApplyGradient(texture, rarityColor);
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
    }
    
    Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => new Color(0.7f, 0.7f, 0.7f, 1f),
            ItemRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f, 1f),
            ItemRarity.Rare => new Color(0.3f, 0.5f, 1f, 1f),
            ItemRarity.Epic => new Color(0.7f, 0.3f, 1f, 1f),
            ItemRarity.Legendary => new Color(1f, 0.6f, 0.1f, 1f),
            _ => Color.white
        };
    }
    
    Color GetWeaponTypeColor(WeaponType type)
    {
        return type switch
        {
            WeaponType.Sword => new Color(0.8f, 0.8f, 0.9f, 1f),
            WeaponType.Axe => new Color(0.6f, 0.3f, 0.2f, 1f),
            WeaponType.Mace => new Color(0.5f, 0.5f, 0.5f, 1f),
            WeaponType.Dagger => new Color(0.7f, 0.7f, 0.8f, 1f),
            WeaponType.Staff => new Color(0.4f, 0.3f, 0.2f, 1f),
            WeaponType.Bow => new Color(0.5f, 0.4f, 0.3f, 1f),
            _ => Color.gray
        };
    }
    
    // FIXED: Updated to use new ArmorType enum values
    Color GetArmorTypeColor(ArmorType type)
    {
        return type switch
        {
            ArmorType.Helmet => new Color(0.6f, 0.6f, 0.7f, 1f),
            ArmorType.Chestplate => new Color(0.5f, 0.5f, 0.6f, 1f),
            ArmorType.Leggings => new Color(0.4f, 0.4f, 0.5f, 1f),
            ArmorType.Gloves => new Color(0.55f, 0.55f, 0.65f, 1f),
            ArmorType.Boots => new Color(0.45f, 0.45f, 0.55f, 1f),
            ArmorType.Shield => new Color(0.65f, 0.6f, 0.5f, 1f), // Bronze shield color
            _ => Color.gray
        };
    }
    
    void FillTexture(Texture2D texture, Color color)
    {
        Color[] pixels = new Color[iconSize * iconSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
    }
    
    void DrawBorder(Texture2D texture, Color color, int width)
    {
        for (int x = 0; x < iconSize; x++)
        {
            for (int y = 0; y < iconSize; y++)
            {
                if (x < width || x >= iconSize - width || y < width || y >= iconSize - width)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }
    
    void DrawWeaponShape(Texture2D texture, WeaponType type, Color color, Color highlightColor)
    {
        int centerX = iconSize / 2;
        int centerY = iconSize / 2;
        int padding = 20;
        
        switch (type)
        {
            case WeaponType.Sword:
                DrawRect(texture, centerX - 4, padding, 8, iconSize - padding * 2 - 20, color);
                DrawTriangle(texture, centerX, padding - 5, 12, color);
                DrawRect(texture, centerX - 20, iconSize - padding - 20, 40, 6, highlightColor);
                DrawRect(texture, centerX - 5, iconSize - padding - 12, 10, 15, new Color(0.4f, 0.25f, 0.15f));
                DrawCircle(texture, centerX, iconSize - padding - 2, 6, highlightColor);
                break;
                
            case WeaponType.Axe:
                DrawRect(texture, centerX - 3, padding, 6, iconSize - padding * 2, new Color(0.5f, 0.35f, 0.2f));
                int bladeTop = padding + 15;
                DrawRect(texture, centerX - 25, bladeTop, 50, 8, color);
                DrawRect(texture, centerX - 30, bladeTop + 8, 50, 20, color);
                for (int i = 0; i < 15; i++)
                {
                    int curveWidth = (int)(25 - i * 1.5f);
                    DrawRect(texture, centerX - 30, bladeTop + 28 + i, curveWidth * 2, 1, color);
                }
                break;
                
            case WeaponType.Mace:
                DrawRect(texture, centerX - 4, padding + 20, 8, iconSize - padding * 2 - 20, new Color(0.4f, 0.3f, 0.2f));
                DrawCircle(texture, centerX, padding + 15, 18, color);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f * Mathf.Deg2Rad;
                    int spikeX = centerX + (int)(Mathf.Cos(angle) * 18);
                    int spikeY = padding + 15 + (int)(Mathf.Sin(angle) * 18);
                    int spikeEndX = centerX + (int)(Mathf.Cos(angle) * 28);
                    int spikeEndY = padding + 15 + (int)(Mathf.Sin(angle) * 28);
                    DrawLine(texture, spikeX, spikeY, spikeEndX, spikeEndY, highlightColor);
                }
                break;
                
            case WeaponType.Dagger:
                DrawRect(texture, centerX - 3, centerY - 25, 6, 35, color);
                DrawTriangle(texture, centerX, centerY - 27, 8, color);
                DrawRect(texture, centerX - 12, centerY + 10, 24, 4, highlightColor);
                for (int i = 0; i < 6; i++)
                {
                    Color handleColor = i % 2 == 0 ? new Color(0.3f, 0.2f, 0.1f) : new Color(0.5f, 0.4f, 0.3f);
                    DrawRect(texture, centerX - 4, centerY + 14 + i * 3, 8, 2, handleColor);
                }
                break;
                
            case WeaponType.Staff:
                int staffWidth = 7;
                DrawRect(texture, centerX - staffWidth/2, padding, staffWidth, iconSize - padding * 2, new Color(0.45f, 0.35f, 0.25f));
                DrawRect(texture, centerX - staffWidth/2 - 1, padding + 10, staffWidth + 2, 3, new Color(0.6f, 0.5f, 0.3f));
                DrawRect(texture, centerX - staffWidth/2 - 1, centerY, staffWidth + 2, 3, new Color(0.6f, 0.5f, 0.3f));
                DrawCircle(texture, centerX, padding + 5, 14, highlightColor);
                DrawCircle(texture, centerX, padding + 5, 10, new Color(highlightColor.r * 0.7f, highlightColor.g * 0.7f, highlightColor.b * 1f, 1f));
                DrawCircle(texture, centerX - 3, padding + 2, 4, new Color(1f, 1f, 1f, 0.8f));
                break;
                
            case WeaponType.Bow:
                int bowTop = padding + 10;
                int bowBottom = iconSize - padding - 10;
                int bowLeft = centerX - 25;
                int bowRight = centerX - 15;
                
                for (int i = 0; i < 40; i++)
                {
                    float t = i / 40f;
                    int x = (int)Mathf.Lerp(bowLeft, bowRight, t);
                    int y = bowTop + i;
                    DrawRect(texture, x, y, 4, 1, color);
                }
                
                for (int i = 0; i < 40; i++)
                {
                    float t = i / 40f;
                    int x = (int)Mathf.Lerp(bowRight, bowLeft, t);
                    int y = centerY + i;
                    DrawRect(texture, x, y, 4, 1, color);
                }
                
                DrawLine(texture, bowLeft, bowTop, bowLeft, bowBottom, new Color(0.9f, 0.9f, 0.9f));
                DrawLine(texture, bowLeft + 1, bowTop, bowLeft + 1, bowBottom, new Color(0.9f, 0.9f, 0.9f));
                
                DrawLine(texture, centerX + 5, centerY, centerX + 30, centerY, new Color(0.6f, 0.4f, 0.2f));
                DrawTriangle(texture, centerX + 32, centerY, 8, new Color(0.7f, 0.7f, 0.7f));
                DrawLine(texture, centerX + 5, centerY - 4, centerX + 5, centerY + 4, new Color(0.8f, 0.2f, 0.2f));
                break;
        }
    }
    
    // FIXED: Updated all case statements to use new ArmorType values
    void DrawArmorShape(Texture2D texture, ArmorType type, Color color, Color highlightColor)
    {
        int centerX = iconSize / 2;
        int centerY = iconSize / 2;
        int padding = 15;
        
        switch (type)
        {
            case ArmorType.Helmet: // Changed from Head
                // Helmet dome
                for (int i = 0; i < 30; i++)
                {
                    int width = (int)(50 - (i * i / 30f));
                    DrawRect(texture, centerX - width/2, padding + 20 + i, width, 1, color);
                }
                DrawRect(texture, centerX - 30, padding + 50, 60, 8, highlightColor);
                DrawRect(texture, centerX - 20, padding + 35, 40, 15, new Color(0.1f, 0.1f, 0.1f));
                DrawRect(texture, centerX - 18, padding + 42, 36, 4, new Color(0.05f, 0.05f, 0.05f));
                DrawRect(texture, centerX - 3, padding + 40, 6, 12, color);
                DrawRect(texture, centerX - 28, padding + 55, 12, 15, color);
                DrawRect(texture, centerX + 16, padding + 55, 12, 15, color);
                break;
                
            case ArmorType.Chestplate: // Changed from Chest
                DrawRect(texture, centerX - 30, centerY - 20, 60, 50, color);
                DrawCircle(texture, centerX - 35, centerY - 15, 15, highlightColor);
                DrawCircle(texture, centerX + 35, centerY - 15, 15, highlightColor);
                DrawRect(texture, centerX - 8, centerY - 20, 16, 50, highlightColor);
                DrawRect(texture, centerX - 32, centerY + 30, 64, 6, new Color(0.3f, 0.25f, 0.2f));
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        DrawCircle(texture, centerX - 20 + i * 20, centerY - 10 + j * 10, 2, new Color(0.3f, 0.3f, 0.3f));
                    }
                }
                break;
                
            case ArmorType.Leggings: // Changed from Legs
                DrawRect(texture, centerX - 22, centerY - 25, 18, 55, color);
                DrawRect(texture, centerX + 4, centerY - 25, 18, 55, color);
                DrawCircle(texture, centerX - 13, centerY + 5, 10, highlightColor);
                DrawCircle(texture, centerX + 13, centerY + 5, 10, highlightColor);
                DrawRect(texture, centerX - 22, centerY - 25, 18, 20, highlightColor);
                DrawRect(texture, centerX + 4, centerY - 25, 18, 20, highlightColor);
                for (int i = 0; i < 3; i++)
                {
                    DrawRect(texture, centerX - 22, centerY + 15 + i * 6, 18, 2, highlightColor);
                    DrawRect(texture, centerX + 4, centerY + 15 + i * 6, 18, 2, highlightColor);
                }
                break;
                
            case ArmorType.Gloves: // Changed from Hands
                int leftX = centerX - 25;
                DrawRect(texture, leftX, centerY - 15, 18, 10, color);
                DrawRect(texture, leftX, centerY - 5, 18, 25, color);
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, leftX + i * 4 + 1, centerY + 20, 3, 12, highlightColor);
                }
                
                int rightX = centerX + 7;
                DrawRect(texture, rightX, centerY - 15, 18, 10, color);
                DrawRect(texture, rightX, centerY - 5, 18, 25, color);
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, rightX + i * 4 + 1, centerY + 20, 3, 12, highlightColor);
                }
                
                for (int i = 0; i < 4; i++)
                {
                    DrawCircle(texture, leftX + 3 + i * 4, centerY + 5, 3, highlightColor);
                    DrawCircle(texture, rightX + 3 + i * 4, centerY + 5, 3, highlightColor);
                }
                break;
                
            case ArmorType.Boots: // Changed from Feet
                int leftBootX = centerX - 28;
                int bootTop = centerY - 15;
                
                DrawRect(texture, leftBootX, bootTop, 20, 28, color);
                DrawRect(texture, leftBootX, bootTop + 28, 22, 8, color);
                DrawRect(texture, leftBootX, bootTop + 36, 28, 16, color);
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, leftBootX + 28 + i, bootTop + 36 + i, 1, 16 - i * 2, highlightColor);
                }
                DrawRect(texture, leftBootX, bootTop + 52, 32, 5, new Color(0.15f, 0.15f, 0.15f));
                
                int rightBootX = centerX + 8;
                DrawRect(texture, rightBootX, bootTop, 20, 28, color);
                DrawRect(texture, rightBootX - 2, bootTop + 28, 22, 8, color);
                DrawRect(texture, rightBootX - 6, bootTop + 36, 28, 16, color);
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, rightBootX - 7 - i, bootTop + 36 + i, 1, 16 - i * 2, highlightColor);
                }
                DrawRect(texture, rightBootX - 10, bootTop + 52, 32, 5, new Color(0.15f, 0.15f, 0.15f));
                
                for (int i = 0; i < 4; i++)
                {
                    DrawLine(texture, leftBootX + 5, bootTop + 5 + i * 6, leftBootX + 15, bootTop + 5 + i * 6, new Color(0.6f, 0.55f, 0.45f));
                    DrawLine(texture, rightBootX + 5, bootTop + 5 + i * 6, rightBootX + 15, bootTop + 5 + i * 6, new Color(0.6f, 0.55f, 0.45f));
                }
                
                DrawRect(texture, leftBootX + 2, bootTop + 48, 8, 4, highlightColor);
                DrawRect(texture, rightBootX + 10, bootTop + 48, 8, 4, highlightColor);
                break;
                
            case ArmorType.Shield: // NEW CASE ADDED
                // Shield face (rounded rectangle)
                DrawRect(texture, centerX - 25, centerY - 30, 50, 60, color);
                // Top curve
                for (int i = 0; i < 10; i++)
                {
                    int width = (int)(50 - i * 2);
                    DrawRect(texture, centerX - width/2, centerY - 30 - i, width, 1, color);
                }
                // Bottom point
                for (int i = 0; i < 15; i++)
                {
                    int width = (int)(50 - i * 3);
                    DrawRect(texture, centerX - width/2, centerY + 30 + i, width, 1, color);
                }
                // Shield boss (center)
                DrawCircle(texture, centerX, centerY, 12, highlightColor);
                DrawCircle(texture, centerX, centerY, 8, color);
                // Border decoration
                DrawRect(texture, centerX - 26, centerY - 25, 3, 50, highlightColor);
                DrawRect(texture, centerX + 23, centerY - 25, 3, 50, highlightColor);
                // Cross emblem
                DrawRect(texture, centerX - 2, centerY - 15, 4, 30, highlightColor);
                DrawRect(texture, centerX - 12, centerY - 2, 24, 4, highlightColor);
                break;
        }
    }
    
    void DrawTriangle(Texture2D texture, int centerX, int tipY, int width, Color color)
    {
        int height = (int)(width * 1.2f);
        for (int y = 0; y < height; y++)
        {
            int lineWidth = (int)((float)y / height * width);
            DrawRect(texture, centerX - lineWidth/2, tipY + y, lineWidth, 1, color);
        }
    }
    
    void DrawCollectibleShape(Texture2D texture, Color color)
    {
        int centerX = iconSize / 2;
        int centerY = iconSize / 2;
        int gemSize = 35;
        
        for (int i = 0; i < gemSize / 2; i++)
        {
            int width = i * 2;
            DrawRect(texture, centerX - width / 2, centerY - gemSize / 2 + i, width, 1, color);
        }
        
        for (int i = 0; i < gemSize / 2; i++)
        {
            int width = (gemSize - i * 2);
            DrawRect(texture, centerX - width / 2, centerY + i, width, 1, color);
        }
        
        Color shineColor = new Color(
            Mathf.Min(color.r + 0.4f, 1f),
            Mathf.Min(color.g + 0.4f, 1f),
            Mathf.Min(color.b + 0.4f, 1f),
            1f
        );
        
        for (int i = 0; i < 8; i++)
        {
            int width = 12 - i;
            DrawRect(texture, centerX - 15 + i, centerY - 15 + i, width, 1, shineColor);
        }
        
        DrawCircle(texture, centerX - 8, centerY - 8, 3, new Color(1f, 1f, 1f, 1f));
    }
    
    void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int px = x + i;
                int py = y + j;
                if (px >= 0 && px < iconSize && py >= 0 && py < iconSize)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }
    
    void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = centerX + x;
                    int py = centerY + y;
                    if (px >= 0 && px < iconSize && py >= 0 && py < iconSize)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }
    }
    
    void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            if (x0 >= 0 && x0 < iconSize && y0 >= 0 && y0 < iconSize)
            {
                texture.SetPixel(x0, y0, color);
            }
            
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
    
    void ApplyGradient(Texture2D texture, Color topColor)
    {
        Color bottomColor = new Color(topColor.r * 0.5f, topColor.g * 0.5f, topColor.b * 0.5f, 0.3f);
        
        for (int y = 0; y < iconSize; y++)
        {
            float t = (float)y / iconSize;
            Color gradientColor = Color.Lerp(topColor, bottomColor, t);
            gradientColor.a = 0.2f;
            
            for (int x = 0; x < iconSize; x++)
            {
                Color currentColor = texture.GetPixel(x, y);
                Color blended = Color.Lerp(currentColor, gradientColor, gradientColor.a);
                texture.SetPixel(x, y, blended);
            }
        }
    }
}