// <copyright file="DecalPropFix.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and boformer. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using UnityEngine;

    /// <summary>
    /// Decal prop fix, a fix for large decals.
    /// </summary>
    internal sealed class DecalPropFix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecalPropFix"/> class.
        /// </summary>
        internal DecalPropFix()
        {
            // Original decal prop fix code by boformer.
            string marker = new Color(12f / 255, 34f / 255, 56f / 255, 1f).ToString();

            // Iterate through all all loaded props.
            for (uint i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); ++i)
            {
                PropInfo prop = PrefabCollection<PropInfo>.GetLoaded(i);

                // Skip null props, props without materials, and non-decal props.
                if (prop?.m_material == null || !prop.m_isDecal)
                {
                    continue;
                }

                // Check for color marker.
                if (prop.m_material.GetColor("_ColorV0").ToString() == marker)
                {
                    // Extact vertex colors.
                    Color colorV1 = prop.m_material.GetColor("_ColorV1");
                    Color colorV2 = prop.m_material.GetColor("_ColorV2");

                    // Update size and tiling.
                    Vector4 size = new Vector4(colorV1.r * 255, colorV1.g * 255, colorV1.b * 255, 0);
                    Vector4 tiling = new Vector4(colorV2.r * 255, 0, colorV2.b * 255, 0);

                    prop.m_material.SetVector("_DecalSize", size);
                    prop.m_material.SetVector("_DecalTiling", tiling);

                    prop.m_lodMaterial.SetVector("_DecalSize", size);
                    prop.m_lodMaterial.SetVector("_DecalTiling", tiling);

                    prop.m_lodMaterialCombined.SetVector("_DecalSize", size);
                    prop.m_lodMaterialCombined.SetVector("_DecalTiling", tiling);
                }
            }
        }
    }
}