<script language=”javascript”> 
function getAttrs() {
    var btn = document.getElementById("xx");
    var str = "";
    for (var i = 0; i < btn.attributes.length; i++) {
        if (i == btn.attributes.length - 1) {
            str = str + btn.attributes[i].name + ":" + btn.attributes[i].value;
        }
        else {
            str = str + btn.attributes[i].name + ":" + btn.attributes[i].value + "**";
        }
    }
    return str;
}
</script>