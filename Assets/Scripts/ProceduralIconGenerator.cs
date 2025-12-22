// ProceduralIconGenerator.cs
// Generates icons for items procedurally
// Add this as a component to GameManager alongside ProceduralItemGenerator

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
        texture.filterMode = FilterMode.Point; // Pixel art style
        
        Color backgroundColor = GetRarityColor(weapon.rarity);
        Color accentColor = GetWeaponTypeColor(weapon.weaponType);
        
        // Fill background
        FillTexture(texture, new Color(0.1f, 0.1f, 0.1f, 1f));
        
        // Add border based on rarity
        if (addBorder)
        {
            DrawBorder(texture, backgroundColor, borderWidth);
        }
        
        // Draw weapon shape
        DrawWeaponShape(texture, weapon.weaponType, accentColor, backgroundColor);
        
        // Add gradient overlay if enabled
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
        
        // Draw gem/collectible shape
        DrawCollectibleShape(texture, rarityColor);
        
        if (useGradients)
        {
            ApplyGradient(texture, rarityColor);
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
    }
    
    // Helper: Get color based on rarity
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
    
    // Helper: Get color based on weapon type
    Color GetWeaponTypeColor(WeaponType type)
    {
        return type switch
        {
            WeaponType.Sword => new Color(0.8f, 0.8f, 0.9f, 1f), // Silver
            WeaponType.Axe => new Color(0.6f, 0.3f, 0.2f, 1f),   // Brown
            WeaponType.Mace => new Color(0.5f, 0.5f, 0.5f, 1f),  // Gray
            WeaponType.Dagger => new Color(0.7f, 0.7f, 0.8f, 1f), // Light silver
            WeaponType.Staff => new Color(0.4f, 0.3f, 0.2f, 1f),  // Wood brown
            WeaponType.Bow => new Color(0.5f, 0.4f, 0.3f, 1f),    // Wood
            _ => Color.gray
        };
    }
    
    // Helper: Get color based on armor type
    Color GetArmorTypeColor(ArmorType type)
    {
        return type switch
        {
            ArmorType.Head => new Color(0.6f, 0.6f, 0.7f, 1f),   // Steel
            ArmorType.Chest => new Color(0.5f, 0.5f, 0.6f, 1f),  // Dark steel
            ArmorType.Legs => new Color(0.4f, 0.4f, 0.5f, 1f),   // Darker steel
            ArmorType.Hands => new Color(0.55f, 0.55f, 0.65f, 1f), // Medium steel
            ArmorType.Feet => new Color(0.45f, 0.45f, 0.55f, 1f),  // Medium dark
            _ => Color.gray
        };
    }
    
    // Fill entire texture with color
    void FillTexture(Texture2D texture, Color color)
    {
        Color[] pixels = new Color[iconSize * iconSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
    }
    
    // Draw border around icon
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
    
    // Draw weapon shapes
    void DrawWeaponShape(Texture2D texture, WeaponType type, Color color, Color highlightColor)
    {
        int centerX = iconSize / 2;
        int centerY = iconSize / 2;
        int padding = 20;
        
        switch (type)
        {
            case WeaponType.Sword:
                // Long blade pointing up
                DrawRect(texture, centerX - 4, padding, 8, iconSize - padding * 2 - 20, color);
                // Sharp point at top
                DrawTriangle(texture, centerX, padding - 5, 12, color);
                // Crossguard
                DrawRect(texture, centerX - 20, iconSize - padding - 20, 40, 6, highlightColor);
                // Handle grip
                DrawRect(texture, centerX - 5, iconSize - padding - 12, 10, 15, new Color(0.4f, 0.25f, 0.15f));
                // Pommel
                DrawCircle(texture, centerX, iconSize - padding - 2, 6, highlightColor);
                break;
                
            case WeaponType.Axe:
                // Long handle
                DrawRect(texture, centerX - 3, padding, 6, iconSize - padding * 2, new Color(0.5f, 0.35f, 0.2f));
                // Large axe head at top - triangle blade
                int bladeTop = padding + 15;
                DrawRect(texture, centerX - 25, bladeTop, 50, 8, color);
                DrawRect(texture, centerX - 30, bladeTop + 8, 50, 20, color);
                // Curved blade edge
                for (int i = 0; i < 15; i++)
                {
                    int curveWidth = (int)(25 - i * 1.5f);
                    DrawRect(texture, centerX - 30, bladeTop + 28 + i, curveWidth * 2, 1, color);
                }
                break;
                
            case WeaponType.Mace:
                // Wooden handle
                DrawRect(texture, centerX - 4, padding + 20, 8, iconSize - padding * 2 - 20, new Color(0.4f, 0.3f, 0.2f));
                // Large spiked head at top
                DrawCircle(texture, centerX, padding + 15, 18, color);
                // Spikes around the head
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
                // Short, thin blade
                DrawRect(texture, centerX - 3, centerY - 25, 6, 35, color);
                // Sharp point
                DrawTriangle(texture, centerX, centerY - 27, 8, color);
                // Small crossguard
                DrawRect(texture, centerX - 12, centerY + 10, 24, 4, highlightColor);
                // Wrapped handle
                for (int i = 0; i < 6; i++)
                {
                    Color handleColor = i % 2 == 0 ? new Color(0.3f, 0.2f, 0.1f) : new Color(0.5f, 0.4f, 0.3f);
                    DrawRect(texture, centerX - 4, centerY + 14 + i * 3, 8, 2, handleColor);
                }
                break;
                
            case WeaponType.Staff:
                // Long wooden staff
                int staffWidth = 7;
                DrawRect(texture, centerX - staffWidth/2, padding, staffWidth, iconSize - padding * 2, new Color(0.45f, 0.35f, 0.25f));
                // Rings/bands on staff
                DrawRect(texture, centerX - staffWidth/2 - 1, padding + 10, staffWidth + 2, 3, new Color(0.6f, 0.5f, 0.3f));
                DrawRect(texture, centerX - staffWidth/2 - 1, centerY, staffWidth + 2, 3, new Color(0.6f, 0.5f, 0.3f));
                // Magical orb at top
                DrawCircle(texture, centerX, padding + 5, 14, highlightColor);
                DrawCircle(texture, centerX, padding + 5, 10, new Color(highlightColor.r * 0.7f, highlightColor.g * 0.7f, highlightColor.b * 1f, 1f));
                // Glow effect
                DrawCircle(texture, centerX - 3, padding + 2, 4, new Color(1f, 1f, 1f, 0.8f));
                break;
                
            case WeaponType.Bow:
                // Draw curved bow limbs
                int bowTop = padding + 10;
                int bowBottom = iconSize - padding - 10;
                int bowLeft = centerX - 25;
                int bowRight = centerX - 15;
                
                // Upper limb curve
                for (int i = 0; i < 40; i++)
                {
                    float t = i / 40f;
                    int x = (int)Mathf.Lerp(bowLeft, bowRight, t);
                    int y = bowTop + i;
                    DrawRect(texture, x, y, 4, 1, color);
                }
                
                // Lower limb curve
                for (int i = 0; i < 40; i++)
                {
                    float t = i / 40f;
                    int x = (int)Mathf.Lerp(bowRight, bowLeft, t);
                    int y = centerY + i;
                    DrawRect(texture, x, y, 4, 1, color);
                }
                
                // Bowstring (tight line on left)
                DrawLine(texture, bowLeft, bowTop, bowLeft, bowBottom, new Color(0.9f, 0.9f, 0.9f));
                DrawLine(texture, bowLeft + 1, bowTop, bowLeft + 1, bowBottom, new Color(0.9f, 0.9f, 0.9f));
                
                // Arrow on string
                DrawLine(texture, centerX + 5, centerY, centerX + 30, centerY, new Color(0.6f, 0.4f, 0.2f));
                DrawTriangle(texture, centerX + 32, centerY, 8, new Color(0.7f, 0.7f, 0.7f));
                // Arrow fletching
                DrawLine(texture, centerX + 5, centerY - 4, centerX + 5, centerY + 4, new Color(0.8f, 0.2f, 0.2f));
                break;
        }
    }
    
    // Draw armor shapes
    void DrawArmorShape(Texture2D texture, ArmorType type, Color color, Color highlightColor)
    {
        int centerX = iconSize / 2;
        int centerY = iconSize / 2;
        int padding = 15;
        
        switch (type)
        {
            case ArmorType.Head:
                // Helmet dome
                for (int i = 0; i < 30; i++)
                {
                    int width = (int)(50 - (i * i / 30f));
                    DrawRect(texture, centerX - width/2, padding + 20 + i, width, 1, color);
                }
                // Helmet base/rim
                DrawRect(texture, centerX - 30, padding + 50, 60, 8, highlightColor);
                // Face opening
                DrawRect(texture, centerX - 20, padding + 35, 40, 15, new Color(0.1f, 0.1f, 0.1f));
                // Visor slit
                DrawRect(texture, centerX - 18, padding + 42, 36, 4, new Color(0.05f, 0.05f, 0.05f));
                // Nose guard
                DrawRect(texture, centerX - 3, padding + 40, 6, 12, color);
                // Cheek guards
                DrawRect(texture, centerX - 28, padding + 55, 12, 15, color);
                DrawRect(texture, centerX + 16, padding + 55, 12, 15, color);
                break;
                
            case ArmorType.Chest:
                // Main torso plate
                DrawRect(texture, centerX - 30, centerY - 20, 60, 50, color);
                // Shoulder plates (pauldrons)
                DrawCircle(texture, centerX - 35, centerY - 15, 15, highlightColor);
                DrawCircle(texture, centerX + 35, centerY - 15, 15, highlightColor);
                // Center breastplate detail
                DrawRect(texture, centerX - 8, centerY - 20, 16, 50, highlightColor);
                // Belt
                DrawRect(texture, centerX - 32, centerY + 30, 64, 6, new Color(0.3f, 0.25f, 0.2f));
                // Rivets/studs
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        DrawCircle(texture, centerX - 20 + i * 20, centerY - 10 + j * 10, 2, new Color(0.3f, 0.3f, 0.3f));
                    }
                }
                break;
                
            case ArmorType.Legs:
                // Left leg plate
                DrawRect(texture, centerX - 22, centerY - 25, 18, 55, color);
                // Right leg plate
                DrawRect(texture, centerX + 4, centerY - 25, 18, 55, color);
                // Knee guards
                DrawCircle(texture, centerX - 13, centerY + 5, 10, highlightColor);
                DrawCircle(texture, centerX + 13, centerY + 5, 10, highlightColor);
                // Thigh plates
                DrawRect(texture, centerX - 22, centerY - 25, 18, 20, highlightColor);
                DrawRect(texture, centerX + 4, centerY - 25, 18, 20, highlightColor);
                // Shin guards with ridges
                for (int i = 0; i < 3; i++)
                {
                    DrawRect(texture, centerX - 22, centerY + 15 + i * 6, 18, 2, highlightColor);
                    DrawRect(texture, centerX + 4, centerY + 15 + i * 6, 18, 2, highlightColor);
                }
                break;
                
            case ArmorType.Hands:
                // Left gauntlet
                int leftX = centerX - 25;
                // Wrist guard
                DrawRect(texture, leftX, centerY - 15, 18, 10, color);
                // Hand plates
                DrawRect(texture, leftX, centerY - 5, 18, 25, color);
                // Fingers
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, leftX + i * 4 + 1, centerY + 20, 3, 12, highlightColor);
                }
                
                // Right gauntlet (mirrored)
                int rightX = centerX + 7;
                DrawRect(texture, rightX, centerY - 15, 18, 10, color);
                DrawRect(texture, rightX, centerY - 5, 18, 25, color);
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, rightX + i * 4 + 1, centerY + 20, 3, 12, highlightColor);
                }
                
                // Knuckle guards
                for (int i = 0; i < 4; i++)
                {
                    DrawCircle(texture, leftX + 3 + i * 4, centerY + 5, 3, highlightColor);
                    DrawCircle(texture, rightX + 3 + i * 4, centerY + 5, 3, highlightColor);
                }
                break;
                
            case ArmorType.Feet:
                // Left boot - more boot-like shape
                int leftBootX = centerX - 28;
                int bootTop = centerY - 15;
                
                // Left boot shaft (tall part)
                DrawRect(texture, leftBootX, bootTop, 20, 28, color);
                // Left boot ankle area
                DrawRect(texture, leftBootX, bootTop + 28, 22, 8, color);
                // Left boot foot (horizontal part pointing right)
                DrawRect(texture, leftBootX, bootTop + 36, 28, 16, color);
                // Left boot toe cap
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, leftBootX + 28 + i, bootTop + 36 + i, 1, 16 - i * 2, highlightColor);
                }
                // Left sole (dark bottom)
                DrawRect(texture, leftBootX, bootTop + 52, 32, 5, new Color(0.15f, 0.15f, 0.15f));
                
                // Right boot - mirrored
                int rightBootX = centerX + 8;
                // Right boot shaft
                DrawRect(texture, rightBootX, bootTop, 20, 28, color);
                // Right boot ankle
                DrawRect(texture, rightBootX - 2, bootTop + 28, 22, 8, color);
                // Right boot foot
                DrawRect(texture, rightBootX - 6, bootTop + 36, 28, 16, color);
                // Right boot toe cap
                for (int i = 0; i < 4; i++)
                {
                    DrawRect(texture, rightBootX - 7 - i, bootTop + 36 + i, 1, 16 - i * 2, highlightColor);
                }
                // Right sole
                DrawRect(texture, rightBootX - 10, bootTop + 52, 32, 5, new Color(0.15f, 0.15f, 0.15f));
                
                // Laces/detail on both boots
                for (int i = 0; i < 4; i++)
                {
                    // Left boot laces
                    DrawLine(texture, leftBootX + 5, bootTop + 5 + i * 6, leftBootX + 15, bootTop + 5 + i * 6, new Color(0.6f, 0.55f, 0.45f));
                    // Right boot laces  
                    DrawLine(texture, rightBootX + 5, bootTop + 5 + i * 6, rightBootX + 15, bootTop + 5 + i * 6, new Color(0.6f, 0.55f, 0.45f));
                }
                
                // Boot heel (back part higher)
                DrawRect(texture, leftBootX + 2, bootTop + 48, 8, 4, highlightColor);
                DrawRect(texture, rightBootX + 10, bootTop + 48, 8, 4, highlightColor);
                break;
        }
    }
    
    // Helper: Draw filled triangle pointing up
    void DrawTriangle(Texture2D texture, int centerX, int tipY, int width, Color color)
    {
        int height = (int)(width * 1.2f);
        for (int y = 0; y < height; y++)
        {
            int lineWidth = (int)((float)y / height * width);
            DrawRect(texture, centerX - lineWidth/2, tipY + y, lineWidth, 1, color);
        }
    }
    
    // Draw collectible shape (gem/crystal)
    void DrawCollectibleShape(Texture2D texture, Color color)
    {
        int centerX = iconSize / 2;
        int centerY = iconSize / 2;
        
        // Draw large gem/crystal shape
        int gemSize = 35;
        
        // Top facet (diamond point)
        for (int i = 0; i < gemSize / 2; i++)
        {
            int width = i * 2;
            DrawRect(texture, centerX - width / 2, centerY - gemSize / 2 + i, width, 1, color);
        }
        
        // Bottom facet (inverted)
        for (int i = 0; i < gemSize / 2; i++)
        {
            int width = (gemSize - i * 2);
            DrawRect(texture, centerX - width / 2, centerY + i, width, 1, color);
        }
        
        // Highlight shine
        Color shineColor = new Color(
            Mathf.Min(color.r + 0.4f, 1f),
            Mathf.Min(color.g + 0.4f, 1f),
            Mathf.Min(color.b + 0.4f, 1f),
            1f
        );
        
        // Top left highlight
        for (int i = 0; i < 8; i++)
        {
            int width = 12 - i;
            DrawRect(texture, centerX - 15 + i, centerY - 15 + i, width, 1, shineColor);
        }
        
        // Bright sparkle
        DrawCircle(texture, centerX - 8, centerY - 8, 3, new Color(1f, 1f, 1f, 1f));
    }
    
    // Helper: Draw rectangle
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
    
    // Helper: Draw circle
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
    
    // Helper: Draw line
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
    
    // Helper: Draw curve (simplified)
    void DrawCurve(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
    {
        DrawLine(texture, x0, y0 - 20, x0 - 5, y0, color);
        DrawLine(texture, x0 - 5, y0, x0, y0 + 20, color);
        DrawLine(texture, x1, y1 - 20, x1 + 5, y1, color);
        DrawLine(texture, x1 + 5, y1, x1, y1 + 20, color);
    }
    
    // Apply gradient overlay
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