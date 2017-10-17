$(() => onDocumentLoad());

async function onDocumentLoad() {
    setupInputBaseUrl();

    await registerPartials();

    if (new URLSearchParams(location.search).get('deleted') !== 'true')
    {
        loadFeeds();
    }
    else {
        loadDeletedFeeds();
    }
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
    $('#inputBaseUrl').change(e => $.cookie(cookieName, $(e.target).val(), { expires: new Date(99999999999999) }));
}

async function loadFeeds() {
    let template = Handlebars.partials['feedsTable'];
    let data = await $.getJSON("/api/feeds");
    let html = template(data);
    $('#divFeeds').html(html);

    setupDeleteButtons();
    setupSubmitNewButton();

    function setupDeleteButtons() {
        $('body').on('click', '.btnDelete:not(.disabled)', async e => {
            e.preventDefault();
            let me = $(e.target);
            let id = me.data("id");
            me.text('Deleting...').addClass('disabled');
            await $.delete(`/api/feeds/${id}`);
            me.closest('.trFeed').remove();
        });
    }

    function setupSubmitNewButton() {
        $('body').on('keyup', '#divNewFeed', e => {
            if (e.keyCode === 13) {
                $('#btnSubmitFeed').click();
            }
        });
        $('body').on('click', '#btnSubmitFeed:not(.disabled)', async e => {
            e.preventDefault();
            let me = $(e.target);

            let data = {
                name: $('#inputName').val(),
                baseUrl: $('#inputBaseUrl').val(),
                additionalParameters: $('#inputAdditional').val().trim().split(/\s+/).filter(v => v)
            };

            let oldText = me.text();
            me.text('Loading...').addClass('disabled');
            try {
                let newFeed = await $.post('/api/feeds', data);

                $('#inputName').val('');
                $('#inputAdditional').val('');

                let newRow = Handlebars.partials['feedRow'](newFeed);
                $(newRow).appendTo($('.tbodyFeeds'));
            }
            catch (e) {
                if (e.responseJSON) {
                    alert(JSON.stringify(e.responseJSON, null, ' '));
                }
                else {
                    alert(e.responseText);
                }
            }
            finally {
                me.text(oldText).removeClass('disabled');
            }
        });
    }
}

async function loadDeletedFeeds() {
    $('#divNewFeed').hide();
    let template = Handlebars.partials['feedsTable'];
    let data = await $.getJSON("/api/feeds/deleted");
    let html = template(data);
    $('#divFeeds').html(html);

    setupUneleteButtons();

    function setupUneleteButtons() {
        $('body').on('click', '.btnUndelete:not(.disabled)', async e => {
            e.preventDefault();
            let me = $(e.target);
            let id = me.data("id");
            me.text('Undeleting...').addClass('disabled');
            await $.post(`/api/feeds/${id}/undelete`);
            me.closest('.trFeed').remove();
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