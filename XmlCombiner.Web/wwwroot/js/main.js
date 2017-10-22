﻿$(() => onDocumentLoad());

async function onDocumentLoad() {
    await registerPartials();

    directAppContent();
}

function showLoader() {
    $('#divPartialContent').html('<div class="loader centered"></div>');
}

async function directAppContent() {
    try {
        switch (getWindowHash()) {
            case '':
            case '#feedgroups':
                await loadFeedGroupsPageAsync();
                break;
            case '#feedgroup':
                let id = getHashQueryParameter('id');
                await loadFeedGroupPageAsync(id);
                break;
            default:
                throw new Error("Invalid page hash");
        }
    }
    catch (e) {
        alertError(e);
    }
}

async function registerPartials() {
    await $.when(
        registerPartial('feedGroupsTable'),
        registerPartial('feedGroupRow'),
        registerPartial('feedsTable'),
        registerPartial('feedRow')
    );

    async function registerPartial(partialName) {
        let partial = await $.get(`/templates/${partialName}.htm`);
        Handlebars.registerPartial(partialName, Handlebars.compile(partial));
    }
}

function setActiveNav(selector) {
    $('.nav-link').removeClass('active');
    $(selector).addClass('active');
}

async function loadFeedGroupPageAsync(id) {
    showLoader();
    setActiveNav('#navFeedGroup');
    $('#hTitle').text('Feed Group: ');
    let template = Handlebars.partials['feedsTable'];
    let data = await $.getJSON(`/api/feedgroups/${id}`);
    let html = template(data);
    $('#divPartialContent').html(html);
    $('#hTitle').text(`Feed Group: ${data.description}`);
}

async function loadFeedGroupsPageAsync() {
    showLoader();
    setActiveNav('#navFeedGroups');
    $('#hTitle').text('Feed Groups');
    let template = Handlebars.partials['feedGroupsTable'];
    let data = await $.getJSON('/api/feedgroups');
    let html = template(data);
    $('#divPartialContent').html(html);
    $('.inputNewFeedGroupBaseUrl').val($.cookie('baseUrl'));
}

async function loadFeeds() {
    showLoader();
    setActiveNav('#navFeeds');
    $('#hTitle').text('Active Feeds');
    let template = Handlebars.partials['feedsTable'];
    try {
        let data = await $.getJSON("/api/feeds");
        let html = template(data);
        $('#divPartialContent').html(html);
    }
    catch (e) {
        $('#divPartialContent').text('Error loading feeds');
    }
}

$(function setupNavigation() {
    window.onhashchange = e => {
        setTimeout(directAppContent, 50);
    };
});

$(function setupHideFeedGroupButton() {
    $('body').on('click', '.btnHideFeedGroup', async e => {
        e.preventDefault();

        let me = $(e.target);
        let id = me.data("id");
        let originalText = me.text();
        me.text('Hiding...').addClass('disabled');
        try {
            await $.put(`/api/feedgroups/${id}/hide`);
            me.text('Hiding...').addClass('disabled');
            $(`.btnUnhideFeedGroup[data-id=${id}]`).show();
            $(`.btnHideFeedGroup[data-id=${id}]`).hide();
        }
        catch (e) {
            alertError(e);
        }
        finally {
            me.text(originalText);
        }
    });
});

$(function setupUnhideFeedGroupButton() {
    $('body').on('click', '.btnUnhideFeedGroup', async e => {
        e.preventDefault();

        let me = $(e.target);
        let id = me.data("id");
        let originalText = me.text();
        me.text('Hiding...').addClass('disabled');
        try {
            await $.put(`/api/feedgroups/${id}/unhide`);
            me.text('Unhiding...').addClass('disabled');
            $(`.btnUnhideFeedGroup[data-id=${id}]`).hide();
            $(`.btnHideFeedGroup[data-id=${id}]`).show();
        }
        catch (e) {
            alertError(e);
        }
        finally {
            me.text(originalText);
        }
    });
});

$(function setupSubmitNewFeedGroupButton() {
    let handler = e => {
        if (e.keyCode === 13) {
            $('.btnSubmitNewFeedGroup').click();
        }
    };
    $('body').on('keyup', '.inputNewFeedGroupDescription', handler);
    $('body').on('keyup', '.inputNewFeedGroupBaseUrl', handler);

    $('body').on('click', '.btnSubmitNewFeedGroup', async e => {
        e.preventDefault();
        let me = $(e.target);
        let data = {
            description: $('.inputNewFeedGroupDescription').val(),
            baseUrl: $('.inputNewFeedGroupBaseUrl').val()
        };

        let oldText = me.text();
        me.text('Loading...').addClass('disabled');
        try {
            let newFeedGroup = await $.post('/api/feedgroups', data);

            let newRow = Handlebars.partials['feedGroupRow'](newFeedGroup);
            $(newRow).insertBefore($('.trNewFeedGroup'));
        }
        catch (e) {
            alertError(e);
        }
        finally {
            me.text(oldText).removeClass('disabled');
        }
    });
});

$(function setupSubmitNewFeedButton() {
    let keyUpHandler = e => {
        if (e.keyCode === 13) {
            $('.btnSubmitFeed').click();
        }
    };
    $('body').on('keyup', '.inputNewFeedBaseUrl', keyUpHandler);
    $('body').on('keyup', '.inputNewFeedName', keyUpHandler);
    $('body').on('keyup', '.inputNewFeedAdditional', keyUpHandler);

    $('body').on('click', '.btnSubmitFeed:not(.disabled)', async e => {
        e.preventDefault();
        let me = $(e.target);

        let data = {
            name: $('.inputNewFeedName').val(),
            baseUrl: $('.inputNewFeedBaseUrl').val(),
            additionalParameters: $('.inputNewFeedAdditional').val().trim().split(/\s+/).filter(p => p).map(p => { return { parameter: p }; })
        };

        let oldText = me.text();
        me.text('Loading...').addClass('disabled');
        try {
            let id = getHashQueryParameter('id');
            let newFeed = await $.post(`/api/feedgroups/${id}/feeds`, data);

            $('.inputNewFeedName').val('');
            $('.inputNewFeedAdditional').val('');

            let newRow = Handlebars.partials['feedRow'](newFeed);
            $(newRow).insertBefore($('.trNewFeed'));
        }
        catch (e) {
            alertError(e);
        }
        finally {
            me.text(oldText).removeClass('disabled');
        }
    });
});

$(function setupDeleteFeedButtons() {
    $('body').on('click', '.btnDeleteFeed:not(.disabled)', async e => {
        e.preventDefault();
        let me = $(e.target);
        let id = me.data("id");
        me.text('Deleting...').addClass('disabled');
        try {
            await $.delete(`/api/feeds/${id}`);
            me.closest('.trFeed').remove();
        }
        catch (e) {
            alertError(e);
        }
    });
});

$(function setupDeleteFeedGroupButtons() {
    $('body').on('click', '.btnDeleteFeedGroup:not(.disabled)', async e => {
        e.preventDefault();
        let me = $(e.target);
        let id = me.data("id");
        me.text('Deleting...').addClass('disabled');
        try {
            await $.delete(`/api/feedgroups/${id}`);
            me.closest('.trFeedGroup').remove();
        }
        catch (e) {
            alertError(e);
        }
    });
});

function alertError(e) {
    if (e.responseJSON) {
        alert(JSON.stringify(e.responseJSON, null, ' '));
    }
    else if (e.responseText) {
        alert(e.responseText);
    }
    else {
        alert(e);
    }
}

function getWindowHash() {
    return window.location.hash.split('?')[0];
}

function getHashQueryParameter(key) {
    return window.location.hash.split('?')[1].split('&')
        .map(kvp => kvp.split('='))
        .filter(kvp => kvp[0] === key)[0][1];
}

$(function setupInputNewFeedGroupBaseUrl() {
    $('.inputNewFeedGroupBaseUrl').change(e => $.cookie('baseUrl', $(e.target).val(), { expires: new Date(99999999999999) }));
});

$(() => {
    $.post = function (url, data) {
        return $.ajax({
            method: "POST",
            contentType: "application/json",
            url: url,
            data: JSON.stringify(data)
        });
    };

    $.put = function (url, data) {
        return $.ajax({
            method: "PUT",
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