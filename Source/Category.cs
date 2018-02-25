/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Collections.Generic;

namespace Kupogram {

    /// <summary>
    /// Represents a group used for filtering nonogram boards.
    /// </summary>
    public struct Category {

        /// <summary>
        /// Lists all known categories.
        /// </summary>
        public static List<Category> All { get; } = new List<Category> {
            new Category(Guid.Parse("{21f9ebaa-fd9e-4aef-b1c0-7784505332bb}"), "Activities"),
            new Category(Guid.Parse("{f99d6018-5d2a-490e-a6d4-86ba306f66de}"), "Animals"),
            new Category(Guid.Parse("{3685d5ad-d4de-4c64-a909-fed9e354924b}"), "Anime & Manga"),
            new Category(Guid.Parse("{e812e23e-53c6-4a48-a2c5-311ab0a0cbc2}"), "Food"),
            new Category(Guid.Parse("{ca902d53-9c6b-4278-8ca3-2a58271140e6}"), "Fun"),
            new Category(Guid.Parse("{a4659542-38fa-4163-881c-1bdd72abfc3b}"), "Games & Media"),
            new Category(Guid.Parse("{8d5e0425-4a99-4a4f-a510-3f08b3fb0bef}"), "Mythology"),
            new Category(Guid.Parse("{3b2ff551-cb7e-4233-abab-446820a6315e}"), "Nature"),
            new Category(Guid.Parse("{3a0b6445-20a3-4c18-b465-b083ec01d5bc}"), "People"),
            new Category(Guid.Parse("{e3c71c46-f4f3-4ff5-a0a1-a987be0cf2ec}"), "Symbols"),
            new Category(Guid.Parse("{35818fdf-4af3-4788-a4ea-2158375360fd}"), "Traditions"),
            new Category(Guid.Parse("{5dda4f68-c8ee-4657-aa76-95b22e007533}"), "Transport"),
            new Category(Guid.Parse("{dde4b597-5384-45c8-9900-65e982e33044}"), "Urban & City"),
            new Category(Guid.Parse("{33b3dd03-ace0-47c0-9da2-e00d8de01996}"), "Weapons")
        };


        /// <summary>
        /// Represents this category's unique identifier.
        /// </summary>
        public Guid ID { get; }

        /// <summary>
        /// Gets or sets a UI-friendly name for this category.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructs a new <see cref="Category"/> object.
        /// </summary>
        public Category(Guid id, string name) {
            ID = id;
            Name = name;
        }

    }

}
