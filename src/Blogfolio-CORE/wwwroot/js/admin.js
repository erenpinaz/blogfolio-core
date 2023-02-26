/* ======================================================
    Filename    : admin.js
    Description : Admin scripts
    Author      : Nejdet Eren Pinaz
   ====================================================== */

if (!window.jQuery) {
    throw "Admin scripts requires jQuery.";
}

$(function () {

    /* ====================
        Initializers
       ==================== */

    // WYSIWYG configuration
    var configureWYGBodyEditor = function () {
        var $wygBody = $("textarea[data-wysiwyg='true']");
        if ($wygBody.length > 0) {

            $wygBody.trumbowyg({
                autogrow: true
            });
        }
    };
    configureWYGBodyEditor();

    // Slug formatting
    configureSlugFormatting();

    // Bind data-modal events
    var configureModalEvents = function () {
        var $modal = $("a[data-modal]");
        if ($modal.length > 0) {
            $modal.each(function () {
                $(this).on("click", function (e) {
                    e.preventDefault();
                    showModal(this.href);
                });
            });
        }
    };
    configureModalEvents();

    // Create & sort social items
    var configureSocialItemContainer = function () {
        // Rubaxa sortable
        var socialItemsContainer = document.getElementById("social-items-container");
        if (socialItemsContainer != null) {
            Sortable.create(socialItemsContainer, {
                handle: "i.drag-handle"
            });
            parseSocialItems();
        }

        // Dynamic item creation
        var $addSocialItem = $("a[data-ajax='add-social-item']");
        var $socialItemsContainer = $("#social-items-container");

        if ($addSocialItem.length > 0 && $socialItemsContainer.length > 0) {
            $addSocialItem.on("click", function (e) {
                e.preventDefault();

                $.ajax({
                    url: this.href,
                    success: function (html) {
                        $(html).appendTo($socialItemsContainer)
                            .find("input[readonly]")
                            .prop("readonly", false);

                        // Parse dynamically added items for validations and icons
                        parseSocialItems();
                    }
                });
                return false;
            });
        }
    };
    configureSocialItemContainer();

    // Media Uploader (plupload)
    var configureMediaUploader = function () {
        var $elem = $('#media-uploader');
        var url = $elem.data('url');

        if ($elem.length) {
            $('#media-uploader').dropzone({
                url: url,
                maxFilesize: 5
            });
        }
    };
    configureMediaUploader();

    // Image Browser
    var configureImageBrowser = function () {
        var $browser = $('div#imageBrowser');
        var $trigger = $('div#preview > button');

        if ($trigger.length) {
            $trigger.on('click', function (e) {
                var url = $(this).data('url');
                var $container = $('div[class="card-deck"]', $browser);

                $.ajax({
                    url: url,
                    success: function (data) {
                        if ($container.length) {
                            $container.empty();

                            $.each(data, function (i, e) {
                                var $card = $([
                                    '<div class="col-sm-4 col-md-4 col-lg-3 p-0 mb-5">',
                                    '<div class="card" data-selection="' + e.path + '">',
                                    '<span class="card-img-top" style="background-image: url(' + e.thumbpath + ');"></span>',
                                    '<div class="card-footer"><small>' + e.name + '</small></div>',
                                    '</div>',
                                    '</div>'
                                ].join(' '));

                                $card.appendTo($container);
                            });

                            $browser.modal();

                            $('.card', $container).off('click');
                            $('.card', $container).on('click', function (e) {
                                $('.card', $container).removeClass('bg-primary text-white');
                                $(this).addClass('bg-primary text-white');
                            });

                            $('.select-image').off('click');
                            $('.select-image').on('click', function (e) {
                                var path = $('.card.bg-primary', $container).data('selection');
                                if (path === undefined || path === '' || path.length <= 0) {
                                    alert('You must select a file before proceeding.');
                                }

                                $('div#preview > input#Image').val(path);
                                $('div#preview > img').attr('src', path);
                            });
                        }
                    }
                });
            });
        }
    };
    configureImageBrowser();
});


/* ====================
    Functions
   ==================== */

// 
// Slug Formatting
/* 
 * Usage: Put "data-slug=source" to the source input and 
 * "data-slug=field" to the input that needs to be slugified
 */
function configureSlugFormatting() {
    var $slugSource = $("input[data-slug='source']");
    var $slugField = $("input[data-slug='field']");
    if ($slugSource.length > 0 && $slugField.length > 0) {
        $slugSource.on("keypress", function () {
            $slugField.val(convertToSlug($(this).val()));
        });
        $slugSource.on("blur", function () {
            $slugField.val(convertToSlug($(this).val()));
        });
        $slugField.on("blur", function () {
            $(this).val(convertToSlug($(this).val()));
        });
    }
}

// Populate Post Categories
function populatePostCategories(postId) {
    var $refresh = $("button[data-refresh=\"category\"]");
    var $list = $("ul[data-list=\"category\"]");

    if ($refresh.length > 0) {
        $refresh.on("click", function () {
            if ($list.length > 0) {
                $.ajax({
                    cache: false,
                    url: "/admin/dashboard/populatepostcategories",
                    type: "GET",
                    data: {
                        id: postId
                    },
                    complete: function (result) {
                        if (result.status === 200) {
                            $list.empty();
                            var categories = result.responseJSON;
                            if (categories.length === 0) {
                                $list.append("<li>No categories</li>");
                            }
                            $(categories).each(function () {
                                var $listItem = $([
                                    "<li class=\"list-group-item\">",
                                    "<div class=\"custom-control custom-checkbox\">",
                                    "<input id=cb_" + this.id + " class=\"custom-control-input\" type=\"checkbox\" name=\"selectedCategories\" value=\"" + this.id + "\" " + (this.ischecked ? "checked" : null) + ">",
                                    "<label for=\"cb_" + this.id + "\" class=\"custom-control-label\">" + this.name + "</label>",
                                    "</div>",
                                    "</li>"
                                ].join(""));
                                $listItem.prependTo($list);
                            });
                        }
                    }
                });
            }
        }).trigger("click");
    }
}

// Show Modal
function showModal(href) {
    var $modalBase = $("#modal-base");
    if ($modalBase.length > 0) {
        var processData = function () {
            var $modalForm = $modalBase.find("form").first();
            if ($modalForm.length === 0) {
                return false;
            }

            // Parse for validations
            $.validator.unobtrusive.parse($modalForm);

            // Parse for slug formatting
            configureSlugFormatting();

            $modalForm.on("submit", function () {
                if (!$(this).valid())
                    return false;

                $.ajax({
                    cache: false,
                    url: href,
                    data: $modalForm.serialize(),
                    type: "POST",
                    success: function (result) {
                        if (result.success) {
                            $modalBase.modal("hide");
                            var $refresh = $("button[data-refresh]");
                            if ($refresh.length > 0) {
                                $refresh.trigger("click");
                            }
                        }
                        $modalBase.html(result);
                        processData();
                    }
                });
                return false;
            });
            return false;
        };

        $modalBase.load(href, function (response, status, xhr) {
            if (status === "success") {
                $modalBase.modal({
                    backdrop: "static",
                    keyboard: false
                }, "show");
                processData();
            }
        });
    }
}

// Parse Social Items
function parseSocialItems() {

    // Initialize icon picker
    $('.social-icon-picker').iconpicker({
        animation: false
    });

    // Parse the form
    var $form = $("form");
    if ($form.length > 0) {
        $form.removeData("validator");
        $form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse($form);
    }

    // Activate delete-item button
    var $deleteSocialItem = $("a[data-ajax='delete-social-item']");
    $deleteSocialItem.on("click", function (e) {
        e.preventDefault();

        $(this).closest("li").remove();
    });
}

/* ====================
    Table Formatters
   ==================== */

// Post Title Formatter
function postTitleFormatter(value, row) {
    return [
        value,
        "<ul class=\"list-inline\">",
        "<li class=\"list-inline-item\"><a class=\"edit\" href=\"/admin/dashboard/updatepost/" + row.postid + "\">Edit</a></li>",
        "<li class=\"list-inline-item\"><a class=\"remove\" href=\"/admin/dashboard/deletepost/" + row.postid + "\">Delete</a></li>",
        "</ul>"
    ].join("");
}

// Post Summary Formatter
function postSummaryFormatter(value) {
    var limit = 36;
    if (value.length > limit) {
        return value.substring(0, limit) + "...";
    }
    return value;
}

// Post Categories Formatter
function postCategoriesFormatter(value) {
    var result = "";
    for (var i = 0; i < value.length; i++) {
        result += "<span class=\"badge badge-primary\" style='font-size: 14px;'>" + value[i].name + "</span> ";
    }
    return result;
}

// Post Comments Section Formatter
function postCommentsSectionFormatter(value) {
    if (value) {
        return "Enabled";
    }
    return "Disabled";
}

// Category Name Formatter
function categoryNameFormatter(value, row) {
    return [
        value,
        "<ul class=\"list-inline\">",
        "<li class=\"list-inline-item\"><a class=\"edit\" href=\"/admin/dashboard/updatecategory/" + row.categoryid + "\">Edit</a></li>",
        "<li class=\"list-inline-item\"><a class=\"remove\" href=\"/admin/dashboard/deletecategory/" + row.categoryid + "\">Delete</a></li>",
        "</ul>"
    ].join("");
}

// Project Name Formatter
function projectNameFormatter(value, row) {
    return [
        value,
        "<ul class=\"list-inline\">",
        "<li class=\"list-inline-item\"><a class=\"edit\" href=\"/admin/dashboard/updateproject/" + row.projectid + "\">Edit</a></li>",
        "<li class=\"list-inline-item\"><a class=\"remove\" href=\"/admin/dashboard/deleteproject/" + row.projectid + "\">Delete</a></li>",
        "</ul>"
    ].join("");
}

// Project Image Formatter
function projectImageFormatter(value) {
    return "<img src='" + value + "' width='120' alt='Media Image'>";
}

// Media Name Formatter
function mediaNameFormatter(value, row) {
    return [
        value,
        "<ul class=\"list-inline\">",
        "<li class=\"list-inline-item\"><a class=\"remove\" href=\"/admin/dashboard/deletemedia/" + row.mediaid + "\">Delete</a></li>",
        "<li class=\"list-inline-item\"><a class=\"showurl\" href=\"" + row.path + "\">Show URL</a></li>",
        "</ul>"
    ].join("");
}

// Media Thumbnail Formatter
function mediaThumbnailFormatter(value, row) {
    if (value == null) {
        return "<div id='" + row.mediaid + "' class='media-thumb' title='" + row.name + "'>" + "<div class='progress'>" + "<div class='progress-bar progress-bar-striped active' role='progressbar' " + "aria-valuenow='0' aria-valuemin='0' aria-valuemax='100' style='width: 0'>" + "<span class='sr-only'>0 Complete</span>" + "</div>" + "</div>";
    }
    return "<div class='media-thumb' style='background-image: url(" + value + ")' title='" + row.name + "'>";
}

// Media Size Formatter
function mediaSizeFormatter(value) {
    return fileSizeIEC(value);
}

// Json Date Formatter
function jsonDateFormatter(value) {
    if (value !== null && value !== undefined && value.length > 0) {
        var date = new Date(value);
        return "<abbr class='initialism' title='" + date + "'>" + calculateDateDifference(date) + "</abbr>";
    }
    return "-";
}

/* ====================
    Table Events
   ==================== */

// Post Title Events
window.postTitleEvents = {
    'click .remove': function (e, value, row, index) {
        e.preventDefault();

        var id = row.postid;
        if (id != undefined) {
            if (confirm("Proceed with deletion?") === true) {
                $.ajax({
                    url: "/admin/dashboard/deletepost",
                    type: "POST",
                    traditional: true,
                    data: JSON.stringify(id),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        var $table = $("#posts-table");
                        if ($table.length > 0) {
                            $table.bootstrapTable("refresh");
                        }
                    }
                });
            }
        }
    }
};

// Post Toolbar Events
function bulkDeletePost(e) {
    e.preventDefault();

    var $table = $("#posts-table");
    if ($table.length > 0) {
        var selectedItems = $table.bootstrapTable("getSelections");
        if (selectedItems.length > 0) {
            if (confirm("Proceed with bulk operation?") === true) {

                var ids = $.map(selectedItems, function (row) {
                    return row.postid;
                });

                $.ajax({
                    url: "/admin/dashboard/bulkdeletepost",
                    type: "POST",
                    data: JSON.stringify(ids),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        $table.bootstrapTable("refresh");
                    }
                });
            }
        }
    }
}

// Category Name Events
window.categoryNameEvents = {
    'click .edit': function (e, value, row, index) {
        e.preventDefault();

        var href = e.target.href;
        if (href.length > 0) {
            showModal(href);
        }
    },
    'click .remove': function (e, value, row, index) {
        e.preventDefault();

        var id = row.categoryid;
        if (id != undefined) {
            if (confirm("Proceed with deletion? \nOrphaned posts will be deleted in the process.") === true) {
                $.ajax({
                    url: e.target.href,
                    type: "POST",
                    traditional: true,
                    data: JSON.stringify(id),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        var $table = $("#categories-table");
                        if ($table.length > 0) {
                            $table.bootstrapTable("refresh");
                        }
                    }
                });
            }
        }
    }
};

// Category Toolbar Events
function bulkDeleteCategory(e) {
    e.preventDefault();

    var $table = $("#categories-table");
    if ($table.length > 0) {
        var selectedItems = $table.bootstrapTable("getSelections");
        if (selectedItems.length > 0) {
            if (confirm("Proceed with bulk operation? \nOrphaned posts will be deleted in the process.") === true) {

                var ids = $.map(selectedItems, function (row) {
                    return row.categoryid;
                });

                $.ajax({
                    url: "/admin/dashboard/bulkdeletecategory",
                    type: "POST",
                    data: JSON.stringify(ids),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        $table.bootstrapTable("refresh");
                    }
                });
            }
        }
    }
}

// Project Name Events
window.projectNameEvents = {
    'click .remove': function (e, value, row, index) {
        e.preventDefault();

        var id = row.projectid;
        if (id != undefined) {
            if (confirm("Proceed with deletion?") === true) {
                $.ajax({
                    url: "/admin/dashboard/deleteproject",
                    type: "POST",
                    traditional: true,
                    data: JSON.stringify(id),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        var $table = $("#projects-table");
                        if ($table.length > 0) {
                            $table.bootstrapTable("refresh");
                        }
                    }
                });
            }
        }
    }
};

// Project Toolbar Events
function bulkDeleteProject(e) {
    e.preventDefault();

    var $table = $("#projects-table");
    if ($table.length > 0) {
        var selectedItems = $table.bootstrapTable("getSelections");
        if (selectedItems.length > 0) {
            if (confirm("Proceed with bulk operation?") === true) {

                var ids = $.map(selectedItems, function (row) {
                    return row.projectid;
                });

                $.ajax({
                    url: "/admin/dashboard/bulkdeleteproject",
                    type: "POST",
                    data: JSON.stringify(ids),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        $table.bootstrapTable("refresh");
                    }
                });
            }
        }
    }
}

// Media Name Events
window.mediaNameEvents = {
    'click .remove': function (e, value, row, index) {
        e.preventDefault();

        var id = row.mediaid;
        if (id != undefined) {
            if (confirm("Proceed with deletion?") === true) {
                $.ajax({
                    url: e.target.href,
                    type: "POST",
                    traditional: true,
                    data: JSON.stringify(id),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        var $table = $("#media-table");
                        if ($table.length > 0) {
                            $table.bootstrapTable("refresh");
                        }
                    }
                });
            }
        }
    },
    'click .showurl': function (e, value, row, index) {
        e.preventDefault();

        window.prompt("Ctrl+C to copy to clipboard:", row.path);
    }
};

// Media Toolbar Events
function bulkDeleteMedia(e) {
    e.preventDefault();

    var $table = $("#media-table");
    if ($table.length > 0) {
        var selectedItems = $table.bootstrapTable("getSelections");
        if (selectedItems.length > 0) {
            if (confirm("Proceed with bulk operation?") === true) {

                var ids = $.map(selectedItems, function (row) {
                    return row.mediaid;
                });

                $.ajax({
                    url: "/admin/dashboard/bulkdeletemedia",
                    type: "POST",
                    data: JSON.stringify(ids),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    complete: function () {
                        $table.bootstrapTable("refresh");
                    }
                });
            }
        }
    }
}

/* ====================
    Utils
   ==================== */

// String.format
// http://stackoverflow.com/questions/610406/javascript-equivalent-to-printf-string-format
if (!String.format) {
    String.format = function (format) {
        var args = Array.prototype.slice.call(arguments, 1);
        return format.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != "undefined"
                ? args[number]
                : match;
        });
    };
}

// Slug formatter
// http://stackoverflow.com/questions/1053902/how-to-convert-a-title-to-a-url-slug-in-jquery
function convertToSlug(url) {
    return url
        .toLowerCase()
        .replace(/ /g, "-")
        .replace(/[^\w-]+/g, "");
}

// IEC (1024) file size prefix
// http://stackoverflow.com/questions/10420352/converting-file-size-in-bytes-to-human-readable
function fileSizeIEC(a, b, c, d, e) {
    return (b = Math, c = b.log, d = 1024, e = c(a) / c(d) | 0, a / b.pow(d, e)).toFixed(2) + " " + (e ? "KMGTPEZY"[--e] + "iB" : "Bytes");
}

// SI (1000) file size prefix
// http://stackoverflow.com/questions/10420352/converting-file-size-in-bytes-to-human-readable
function fileSizeSI(a, b, c, d, e) {
    return (b = Math, c = b.log, d = 1e3, e = c(a) / c(d) | 0, a / b.pow(d, e)).toFixed(2) + " " + (e ? "kMGTPEZY"[--e] + "B" : "Bytes");
}

// Calculate Date Difference
function calculateDateDifference(dateFrom, dateTo) {
    // Calculate the difference
    dateTo = (typeof dateTo === "undefined") ? (new Date).getTime() : dateTo;
    var days = Math.floor((dateTo - dateFrom) / 1000 / 60 / 60 / 24);

    // Format and return result
    var duration = Math.floor(days / 365.242); // year
    if (duration > 1) {
        return duration + " years ago";
    }
    if (duration === 1) {
        return "a year ago";
    }
    duration = Math.floor(days / 30.4368); // month
    if (duration > 1) {
        return duration + " months ago";
    }
    if (duration === 1) {
        return "a month ago";
    }
    duration = days; // days
    if (duration > 1) {
        return duration + " days ago";
    }
    if (duration === 1) {
        return "yesterday";
    }
    return "today";
}