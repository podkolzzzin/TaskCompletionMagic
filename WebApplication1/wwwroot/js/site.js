// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const chat = {
    
    messageCount: 0,
    
    send: function(message) {
        this.showMessage(message, true);
        $.ajax('/home/SendMessage',
            {
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({msg: message}),
                success: function () {
                    console.log("sent");
                }
            }
        );
    },
    showMessage: function(message, yours) {
        this.messageCount++;
        $('.chat-panel').append('<div class="row no-gutters">'
            + '<div class="col-md-3 '+ (yours ? 'offset-md-9' : '') +'">'
            + '<div class="chat-bubble chat-bubble--'+ (yours ? 'right' : 'left') +'">'
            + message + '</div></div></div>')
    },
    start: function () {
        const that = this;
        $.ajax('/home/WaitMessage',
            {
                type: 'POST',
                contentType : 'application/json',
                data: JSON.stringify({id: this.messageCount}),
                success: function (r) {
                    for (let i = 0; i < r.length; i++)
                        that.showMessage(r[i], false);
                    that.start();
                }
            });
    },
    setup: function () {
        var that = this;
        $('.material-icons').click(function () {
            const text = $('input').val();
            that.send(text);
            $('input').val('');
        })
    }
}

$(document).ready(function() {
    chat.setup();
    chat.start();
})