ChangeRecipeSubPanel.prototype.updateSelectedItem = function(typeId) {
    this.recipesDropdown.reset();
    let recipesListObj = this.parentPanel._getRecipesFromTypeId(typeId);

    let count = 1;
    for (let recipeId in recipesListObj)
    {
        let label = "Recipe".concat(" ", count);

        this.recipesDropdown.addListElement(recipeId, label);
        ++count;
    }

    let hasMultipleRecipes = (Object.keys(recipesListObj).length > 1);
    let defaultSelectedRecipeKey = Object.keys(recipesListObj)[0];
    this.HTMLNodes.recipesArea.classList.toggle("hide", !hasMultipleRecipes);

    this._updateSelectedItemInformation(typeId);
    this.setCurrentChangeRecipe(defaultSelectedRecipeKey);
};