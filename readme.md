# UMT Scripts


## Resource Exporters

### ExportSpriteSizeFilter
This script exports sprites from a game, allowing for filtering based on their dimensions. It provides options to set minimum and maximum width and height for the sprites to be exported. Only sprites that meet the specified size criteria will be exported as PNG files to a designated output directory.

### ExportSpriteSheetsMeta
This script exports sprite sheets from a game. Similarly to ExportSpriteSizeFilter, it first allows for applying a comparison size filter to limit which sprites are exported based on their dimensions. After filtering, it exports the selected sprites as PNG files to a specified output directory.
It also creates a `.meta` file that contains metadata for use with my `AssetLoader` mod.