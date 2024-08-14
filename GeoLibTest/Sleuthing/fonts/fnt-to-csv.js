const fs = require('fs');
const { spawn } = require('child_process');

function getGlyphWidth( glyph ){
    let width = 0;
    for( let inst of glyph.path ){
        width = Math.max(width, inst.x);
    }
    return width;
}

function convert( TO_LOAD, monospace = true ){

    let data = fs.readFileSync(`${TO_LOAD}.fnt`, 'latin1');

    /** @type {string} */
    const file = data.toString();

    const GLYPH_MATCHER   = /#~2\r?\n(.)\r?\n\d+\r?\n((?:#~2\.1\r?\n\d+\r?\n(?:.(?: \d+ \d+)?\r?\n)+\|~\r?\n)+)/gms
    const CONTOUR_MATCHER = /#~2\.1\r?\n\d+\r?\n((?:[MD] \d+ \d+\r?\n)+)(C)?/gms
    const INSTR_MATCHER   = /(?:([MD]) (\d+) (\d+))/gms
    const HEADER_MATCHER  = /#~HEADER\r?\n.*?\r?\n(\d+)\r?\n(\d+)\r?\n(\d+)\r?\n(\d+)\r?\n(\d+)\r?\n(\d+\.\d+)/gm;

    const header = HEADER_MATCHER.exec(file);


    const BASELINE   = parseInt(header[4]);

    const MAGIC_SCALE  = parseFloat(header[6]); // TruTops Inch divides the height by this factor when displaying the font... why??

    const ASCENT     = ( parseInt(header[3]) - BASELINE ) / MAGIC_SCALE;


    let UNITS_PER_EM = 0; // have to calculate this manually
    let CHAR_WIDTH   = 0; // ditto

    const glyphs = [];

    const glyph_blocks = file.matchAll(GLYPH_MATCHER)
    for( const glyph_block of glyph_blocks ){
        let glyph = {
            char: glyph_block[1],
            path: []
        }

        const contour_blocks = glyph_block[2].matchAll(CONTOUR_MATCHER)
        for( const contour_block of contour_blocks ){
            let contour = [...contour_block[1].matchAll(INSTR_MATCHER)].map( i => {
                return {
                    op: i[1],
                    x: parseInt(i[2]),
                    y: parseInt(i[3])
                }
            })
            if(contour_block[2]){
                contour.push({
                    op: 'D',
                    x: contour[0].x,
                    y: contour[0].y
                })
            }
            glyph.path.push(...contour)
        }

        UNITS_PER_EM = glyph.path.reduce( (prev, inst) => Math.max(inst.y, prev), UNITS_PER_EM )
        CHAR_WIDTH   = glyph.path.reduce( (prev, inst) => Math.max(inst.x, prev), CHAR_WIDTH )
        glyphs.push(glyph)
    }

    console.log("char width:", CHAR_WIDTH, "em:", UNITS_PER_EM)

    let jump   = '0/0, 0\n'
    let csv    = '';
    for( let glyph of glyphs ){
        for( inst of glyph.path ){
            if( inst.op === 'M' ){
                csv += jump;
            }
            csv += `${inst.x}, ${inst.y}\n`
        }
        csv += '0/0, 1\n'
    }
    fs.writeFileSync(`${TO_LOAD}.csv`, csv); // https://www.desmos.com/calculator/uyqefbcskn

}

convert('ISO')
convert('ISOPROP')
convert('iso_dim')
convert('BOLD', false)