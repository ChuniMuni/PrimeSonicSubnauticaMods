﻿namespace CustomCraft2SML.Interfaces
{
    using System.Collections.Generic;

    internal interface IFabricatorEntries
    {
        ICollection<string> CustomTabIDs { get; }
        ICollection<string> MovedRecipeIDs { get; }
        ICollection<string> AddedRecipeIDs { get; }
        ICollection<string> AliasRecipesIDs { get; }

        void DuplicateCustomTabDiscovered(string id);
        void DuplicateMovedRecipeDiscovered(string id);
        void DuplicateAddedRecipeDiscovered(string id);
        void DuplicateAliasRecipesDiscovered(string id);
    }
}