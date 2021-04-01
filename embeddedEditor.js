/// ace Editor ///

// Code from
// https://jsbin.com/taniqikagi/edit?html,js,output

var TextHighlightRules = ace.require("ace/mode/text_highlight_rules").TextHighlightRules;
TextHighlightRules.prototype.createKeywordMapper = function(map, defaultToken, ignoreCase, splitChar) 
{
    var keywords = this.$keywords = Object.create(null);

    Object.keys(map).forEach(function(className) 
    {
        var a = map[className];
        if (ignoreCase) 
            a = a.toLowerCase();

        var list = a.split(splitChar || "|");
        for (var i = list.length; i--; )
            keywords[list[i]] = className;
    });

    // in old versions of opera keywords["__proto__"] sets prototype
    // even on objects with __proto__=null
    if (Object.getPrototypeOf(keywords)) 
    {
        keywords.__proto__ = null;
    }

    this.$keywordList = Object.keys(keywords);
    map = null;
    return ignoreCase
            ? function(value) {return keywords[value.toLowerCase()] || defaultToken; }
            : function(value) {return keywords[value] || defaultToken; };
};

var editor = ace.edit("editor", { mode: 'ace/mode/glsl' });

editor.setTheme("ace/theme/monokai");

editor.resize();


function updateKeywords(list) {
    var keywords = editor.session.$mode.$highlightRules.$keywords;
    list.forEach(function(x) 
    {
        keywords[x[0]] = x[1];
    });
    editor.session.bgTokenizer.start(0);
}

setTimeout(function() { 
    updateKeywords([["VAL", "support.type"], 
                    ["VAL2", "support.type"],
                    ["VAL3", "support.type"],
                    ["add", "support.function"],
                    ["sub", "support.function"],
                    ["mul", "support.function"],
                    ["recip", "support.function"],
                    ["div", "support.function"],
                    ["d_sq", "support.function"],
                    ["d_sin", "support.function"],
                    ["d_cos", "support.function"],
                    ["d_exp", "support.function"],
                    ["d_log", "support.function"],
                    ["dc_mul", "support.function"],
                    ["dc_sq", "support.function"],
                    ["dc_conj", "support.function"],
                    ["dc_absSq", "support.function"],
                    ["dcr_div", "support.function"],
                    ["dc_recip", "support.function"]
                ]); 

    // https://github.com/ajaxorg/ace/wiki/Creating-or-Extending-an-Edit-Mode
    //
    // "variable.language" : this
    // "keyword" : types and keyword
    // "constant.language" : max mix clamp dot step
    // "keyword.operator"
    // "support.type"
    //
    /*
    "keyword.control" : keywordControls,
    "storage.type" : storageType,
    "storage.modifier" : storageModifiers,
    */

}, 1000); // TODO


