var uploadEnum = {
    Sucesso: 1,
    Falha: 2,
    Erro: 3
};

function validarArquivoUpload(pArquivoNome) {
    var extensoes_permitidas = new Array("wvm", "mp4", "mpeg-4", "avi", "jpg", "jpeg", "png", "gif", "bmp", "pps", "pdf", "jpg", "zip");
    var arquivo_extensao = pArquivoNome.split('.').pop().toLowerCase();

    for (var i = 0; i <= extensoes_permitidas.length; i++) {
        if (extensoes_permitidas[i] == arquivo_extensao) {
            return true; // extensão arquivo valido
        }
    }
    alert("Não é uma extensão permitida");
    // Eu recebo o elemento de entrada via Id e set value = '';
    document.getElementById('file').value = '';
    return false;
}
$(document).ready(function () {
    $('[data-toggle="popover"]').popover();
});

$(window).on('load', function () {

//jQuery("#loader").delay(2000).fadeOut("slow");

$("button").click(function () {
    var progressEle = $("#progress");
    var carregar = $("#loader");

    var data = document.getElementById("file").files[0];
    if ((!data) || (data.size > 314572800)) {
        alert("Selecione um arquivo !");
        return false;
    }
    var pArquivoNome = data.name;

    if (validarArquivoUpload(pArquivoNome)) {
        //progressEle.css("background", "blue");
        document.getElementById("inserir").disabled = true;
        document.getElementById("file").disabled = true;
        carregar.css("visibility", "visible");
        progressEle.css("background", "#f0f0f0");
        //progressEle.css("visibility", "visible");
        $("#progress").show();
        //progressEle.show();
        //progressEle.hide();   

        var formData = new FormData();

        formData.append("files", data);

        $.ajax({
            url: "/Upload/UploadArquivo",
            async: true,
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var progress = Math.round((evt.loaded / evt.total) * 100);
                        if (progress > 0)
                            progressEle.css("background", "blue");
                        progressEle.width(progress + "%");
                    }
                }, false);
                return xhr;
            },
            success: function (data) {
                if (data.state === 1) {
                    //progressEle.css("background", "green").slideUp(200).delay(2000).fadeIn(400);
                    progressEle.delay(1000);
                    carregar.css("visibility", "hidden");
                    //progressEle.css("display", "none").slideUp(200).delay(2000).fadeIn(400);
                    document.getElementById('file').value = '';
                    alert("Enviado com sucesso!");
                    progressEle.css("background", "green").delay("slow");
                    document.getElementById("inserir").disabled = false;
                    document.getElementById("file").disabled = false;                    
                }
                else {
                    progressEle.css("background", "red").delay("slow");
                }
            }
        });
    }
    });
});


/*
function uploadFiles(inputId) {
    var input = document.getElementById(inputId);
    var files = input.files;
    var formData = new FormData();

    for (var i = 0; i != files.length; i++) {
        formData.append("files", files[i]);
    }

    startUpdatingProgressIndicator();
    $.ajax(
        {
            url: "/upload/UploadArquivo",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                stopUpdatingProgressIndicator();
                alert("Files Uploaded!");
            }
        }
    );
}

var intervalId;

function startUpdatingProgressIndicator() {
    $("#progress").show();

    intervalId = setInterval(
        function () {
            // We use the POST requests here to avoid caching problems (we could use the GET requests and disable the cache instead)
            $.post(
                "/upload/Progresss",
                function (progress) {
                    $("#bar").css({ width: progress + "%" });
                    $("#label").html(progress + "%");
                }
            );
        },
        10
    );
}

function stopUpdatingProgressIndicator() {
    clearInterval(intervalId);
}
*/