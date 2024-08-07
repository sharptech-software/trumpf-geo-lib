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
                    y: (parseInt(i[3]) - BASELINE) / MAGIC_SCALE
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

    // let jump   = '0/0, 0/0\n'
    // let csv    = '';
    // let cursor = 0;
    // for( let glyph of glyphs ){
    //     for( inst of glyph.path ){
    //         if( inst.op === 'M' ){
    //             csv += jump;
    //         }
    //         csv += `${inst.x + cursor}, ${inst.y}\n`
    //     }
    //     cursor += CHAR_WIDTH;
    //     csv += jump
    // }
    // fs.writeFileSync(`${TO_LOAD}.csv`, csv); // https://www.desmos.com/calculator/uyqefbcskn


    let svgglyphs = '';
    for( let glyph of glyphs ){
        let path = '';
        let char = glyph.char.replace(/[\u00A0-\u9999<>\&]/g, function(i) {
            return '&#'+i.charCodeAt(0)+';';
        });
        for( inst of glyph.path ){
            let svg_inst = inst.op == 'M' ? 'M' : 'L';
            path += `${svg_inst}${inst.x},${inst.y} `;
        }
        svgglyphs += `            <glyph unicode="${encodeURI(char)}" horiz-adv-x="${monospace ? CHAR_WIDTH : getGlyphWidth(glyph) + 10}" d="${path}"/>\n`
    }

    fs.writeFileSync(`${TO_LOAD}.svg`, `
    <!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN" "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd" >
    <svg xmlns="http://www.w3.org/2000/svg">
        <defs>
            <font id="${TO_LOAD}" horiz-adv-x="${CHAR_WIDTH}">
                <font-face font-family="${TO_LOAD}" units-per-em="${UNITS_PER_EM}" ascent="${ASCENT}"/>
    ${svgglyphs}
            </font>
        </defs>
    </svg>
    `);

    const fontforge = spawn('fontforge', ['-script', 'expand_strokes.py', `${TO_LOAD}.svg`, `${TO_LOAD}.otf`, CHAR_WIDTH]);
    fontforge.stdout.on('data', (data) => {
        console.log(`${data}`);
    });
    fontforge.stderr.on('data', (data) => {
        console.error(`${data}`);
    });
    // fontforge.on('close', (code) => {
    //     const view = spawn('fontforge', [`${TO_LOAD}.otf`]);
    // });

}

convert('ISO')
convert('ISOPROP')
convert('iso_dim')
convert('BOLD', false)