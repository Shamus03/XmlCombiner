$(() => onDocumentLoad());

async function onDocumentLoad() {
    await registerPartials();

    loadFeeds();

    setupInputBaseUrl();
}

async function registerPartials() {
    await $.when(
        registerPartial('feedsTable'),
        registerPartial('feedRow')
    );
}

async function registerPartial(partialName) {
    let partial = await $.get(`/templates/${partialName}.htm`);
    Handlebars.registerPartial(partialName, Handlebars.compile(partial));
}

function setupInputBaseUrl() {
    let cookieName = "baseUrl";
    $('#inputBaseUrl').val($.cookie(cookieName));
    $('#inputBaseUrl').change(e => $.cookie(cookieName, $(e.target).val()));
}

async function loadFeeds() {
    let template = Handlebars.partials['feedsTable'];
    let html = template(await $.getJSON("/api/feeds"));
    $('#divFeeds').html(html);

    setupDeleteButtons();
    setupSubmitNewButton();

    function setupDeleteButtons() {
        $('body').on('click', '.btnDelete', async e => {
            e.preventDefault();
            let me = $(e.target);
            let id = me.data("id");
            me.text('Deleting...').prop('disabled', true);
            await $.delete(`/api/feeds/${id}`);
            me.closest('.trFeed').remove();
        });
    }

    function setupSubmitNewButton() {
        $('.btnSubmitFeed').click(async e => {
            e.preventDefault();
            let me = $(e.target);
            let newFeedRow = $('.trNewFeed');
            let name = $('.inputName').val();
            let additional = $('.inputAdditional').val().split(" ");

            let data = {
                name: $('.inputName').val(),
                baseUrl: $('#inputBaseUrl').val(),
                additionalParameters: $('.inputAdditional').val().split(" ")
            };

            let oldText = me.text();
            me.text('Loading...').prop('disabled', true);
            try {
                let newFeed = await $.post('/api/feeds', data);

                newFeedRow.find('input').val('');

                let newRow = Handlebars.partials['feedRow'](newFeed);
                $(newRow).insertBefore(newFeedRow);
            }
            finally {
                me.text(oldText).prop('disabled', false);
            }
        });
    }
}

$(() => {
    $.post = function (url, data) {
        return $.ajax({
            method: "POST",
            contentType: "application/json",
            url: url,
            data: JSON.stringify(data)
        });
    };

    $.delete = function (url) {
        return $.ajax({
            method: "DELETE",
            url: url
        });
    };
});