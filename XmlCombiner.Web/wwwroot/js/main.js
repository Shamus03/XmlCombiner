$(() => onDocumentLoad());

async function onDocumentLoad() {
    setupNavigation();

    setupInputBaseUrl();

    await registerPartials();

    directAppContent();
}

function directAppContent() {
    if (window.location.hash === '#deleted') {
        loadDeletedFeeds();
    }
    else {
        loadFeeds();
    }
}

function setupNavigation() {
    $('#navFeeds').click(e => {
        loadFeeds();
    });
    $('#navDeleted').click(e => {
        loadDeletedFeeds();
    });

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

function setActiveNav(selector) {
    $('.nav-link').removeClass('active');
    $(selector).addClass('active');
}

async function loadFeeds() {
    setActiveNav('#navFeeds');
    $('#divNewFeed').show();
    $('#hTitle').text('Active Feeds');
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
    setActiveNav('#navDeleted');
    $('#divNewFeed').hide();
    $('#hTitle').text('Deleted Feeds');
    let template = Handlebars.partials['feedsTable'];
    let data = await $.getJSON("/api/feeds/deleted");
    let html = template(data);
    $('#divFeeds').html(html);

    setupUneleteButtons();
    setupDeleteForeverButtons();

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

    function setupDeleteForeverButtons() {
        $('body').on('click', '.btnDeleteForever:not(.disabled)', async e => {
            e.preventDefault();
            let me = $(e.target);
            let id = me.data("id");
            me.text('Deleting forever...').addClass('disabled');
            await $.post(`/api/feeds/${id}/deleteforever`);
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