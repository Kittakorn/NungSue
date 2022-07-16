function UpdateFavorite(e, remove) {
    var bookId = $(e).attr("value");
    var request = $.ajax({
        type: 'GET',
        url: '/like/' + bookId,
        dataType: "json"
    });

    request.done(
        function (response) {
            if (response) {
                $(e).removeClass('text-muted').addClass('text-danger');
                var notyf = new Notyf();
                notyf.success({
                    message: 'เพิ่มหนังสือในรายการโปรดแล้ว',
                    dismissible: true
                })
            }
            else {
                if (remove) $('#' + bookId).remove();
                else $(e).removeClass('text-danger').addClass('text-muted');
            }
        }
    );

    request.catch(
        function () {
            window.location.href = '/account/sign-in?ReturnUrl=' + encodeURIComponent(window.location.pathname);
        }
    );
}

function AddBookToCart(e) {
    var bookId = $(e).attr("value");
    var request = $.ajax({
        type: 'GET',
        url: '/cart/add/' + bookId,
        dataType: "json"
    });

    request.done(
        function (count) {
            if (count > 0)
                $("#cart-count").removeClass('d-none').html(count);
            else
                $("#cart-count").addClass('d-none');

            var notyf = new Notyf();
            notyf.success({
                message: 'เพิ่มหนังสือลงตะกร้าสินค้าแล้ว',
                dismissible: true
            })
        }
    );

    request.catch(
        function () {
            window.location.href = '/account/sign-in?ReturnUrl=' + encodeURIComponent(window.location.pathname);
        }
    );
}



