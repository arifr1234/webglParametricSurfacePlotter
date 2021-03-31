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

function updateKeywords(list) {
    var keywords = editor.session.$mode.$highlightRules.$keywords;
    list.forEach(function(x) 
    {
        keywords[x[0]] = x[1];
    });
    editor.session.bgTokenizer.start(0);
}

setTimeout(function() { updateKeywords([["foo", "keyword"], ["barb", "keyword"]]); }, 1000); // TODO
// "variable.language"
// "keyword"
// "constant.language"


