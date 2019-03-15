var uploadEnum = {
    Sucesso: 1,
    Falha: 2,
    Erro: 3
};

$("button").click(function () {
    var progressEle = $("#progress");
    progressEle.css("display", "block").delay("slow");
    progressEle.css("background", "blue");

    var data = document.getElementById("file").files[0];

    var formData = new FormData();

    formData.append("files", data);

    startUpdatingProgressIndicator();

    $.ajax({
        url: "/CanalIlhas/UploadArquivo",
        data: formData,
        processData: false,
        contentType: false,
        type: "POST",
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var progress = Math.round((evt.loaded / evt.total) * 100);
                    progressEle.width(progress + "%");
                }
            }, false);
            return xhr;
        },
        success: function (data) {
            if (data.state === 1) {
                //progressEle.css("background", "green").slideUp(200).delay(2000).fadeIn(400);
                progressEle.css("background", "green").delay("slow");
                //progressEle.css("display", "none").slideUp(200).delay(2000).fadeIn(400);
                stopUpdatingProgressIndicator();
                alert("Files Uploaded!");
            }
            else {
                progressEle.css("background", "red").delay("slow");
                progressEle.css("display", "none");
            }
        }
    });
});

//$(document).ready(function () {
//    $('[data-toggle="popover"]').popover();
//});

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
            url: "/upload",
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
                "/upload/progress",
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