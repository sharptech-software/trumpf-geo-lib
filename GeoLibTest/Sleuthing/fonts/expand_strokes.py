#!/usr/bin/env fontforge
import fontforge
import sys

def stroke_glyphs(font_path, output_path, space_width):
    # Open the font
    font = fontforge.open(font_path)

    font.os2_version = 4
    font.os2_width = 9
    font.os2_stylemap = font.os2_stylemap | 0x0001
    
    # Iterate through all glyphs
    for glyph in font.glyphs():
        # Skip empty glyphs
        if glyph.isWorthOutputting():            
            # Stroke the glyph
            glyph.stroke("elliptical", 1.5, 1.5, cap="round", join="round", accuracy=0.01, removeoverlap="none")
            glyph.removeOverlap()
            glyph.correctDirection()

    # Fix space width
    if "space" in font:
        font.removeGlyph("space")
    
    space_glyph = font.createChar(0x0020, "space")
    space_glyph.width = space_width

    # Generate the new font
    font.generate(output_path)
    print(f"Stroked font saved as {output_path}")

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("Usage: fontforge -script script.py input_font.svg output_font.otf space_width")
        sys.exit(1)

    input_font = sys.argv[1]
    output_font = sys.argv[2]
    space_width = int(sys.argv[3])

    stroke_glyphs(input_font, output_font, space_width)