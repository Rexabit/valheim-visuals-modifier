## Details

### How Valheim Binds Armors to a Character
* Skins are attached to a player by looking for 'attach_skin' and 'attach' objects within an Item GameObject.
* Within the item is a GameObject (default) that contains the visual information of an item dropped on the ground.
* The ItemDrop component contains information about the material: m_itemData.m_shared.m_armorMaterial

### Item Structure (Armors)
```
- Item
    - Component[ItemDrop]
    - default
    - attach_skin
- Body
    - body
        - Shader (Custom/Player)
            - _MainTex
            - _ChestBumpMap
            - _ChestMetal
            - _ChestTex
            - _LegsBumpMap
            - _LegsMetal
            - _LegsTex
```

### Item Structure (Weapons)
```
- Item
    - Component[ItemDrop]
    - default
        - Component[MeshRenderer]
    - attach
        - * Component[SkinnedMeshRenderer]
```

### Creature structure
```
- Monster
    - Visual
        - VisualPieces[SkinnedMeshRenderer]
```