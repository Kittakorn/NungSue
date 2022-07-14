function UpdateFavorite(e) {
    var bookId = $(e).attr("value");
    var request = $.ajax({
        type: 'GET',
        url: '/customer/favorite/' + bookId,
        dataType: "json"
    });

    request.done(
        function (response) {
            if (response == true) {
                $(e).removeClass('text-muted').addClass('text-danger');
                var notyf = new Notyf();
                notyf.success({
                    message: 'เพิ่มหนังสือในรายการโปรดแล้ว',
                    dismissible: true
                })
            }
            else {
                $(e).removeClass('text-danger').addClass('text-muted');
            }
        }
    );

    request.catch(
        function () {
            window.location.href = '/account/sign-in?ReturnUrl=' + encodeURIComponent(window.location.pathname);
        }
    );
}

