'use strict';

import * as vscode from 'vscode';
import * as opn from 'opn';
import * as fs from 'fs';
import * as path from 'path';
import * as restify from 'restify';

const server = restify.createServer();
const PORT = 4567;

server.use(restify.plugins.queryParser());
server.use(
    function crossOrigin(req,res,next){
      res.header("Access-Control-Allow-Origin", "*");
      res.header("Access-Control-Allow-Headers", "X-Requested-With");
      return next();
    }
);

server.listen(PORT, () => {
    console.log(`Extension server online on port ${PORT}`);
})

// Serve the bot content.
server.get('/', restify.plugins.serveStatic({
    'directory': `${__dirname}/bot`,
    'default': 'bot.html'
}));

// Opens URLs.
server.get('/open', (req, res, next) => {
    if (!req.query.url) {
        res.send(200);
        return;
    }

    opn(req.query.url);
    res.send(200);
});

// This is the function that gets run when the VS Code extension
// gets loaded. activate is triggered by a registered activation event
// defined in package.json.
export function activate(context: vscode.ExtensionContext) {
    // Provides the content for the previewHtml pane.
    context.subscriptions.push(
      vscode.workspace.registerTextDocumentContentProvider('stobot', stobotContent)
    );

    // Registers a handler for the 'startBot' command defined in package.json.
    let disposable = vscode.commands.registerCommand('stobot.startBot', () => {
        vscode.commands.executeCommand('vscode.previewHtml', 'stobot://stobot', vscode.ViewColumn.Two, 'Stack Overflow Bot');
    });

    context.subscriptions.push(disposable);
};

const stobotContent = {
    botToken: "",
    htmlBotContent: () => {
        const BOT_TOKEN = vscode.workspace.getConfiguration('StackCode').get('directLineToken');
        if (!BOT_TOKEN) {
            return `
            <!doctype html>
            <html>
                <meta charset="utf-8"/>
                <body>
                    <h2>Please set StackCode.directLineToken in your user settings!</h2>
                </body>
            </html>`
        }

        return `
        <!doctype html>
        <html>
            <meta charset="utf-8"/>
            <head>
                <style>
                    html,body {
                        background-color: white;
                        width: 100%;
                        height: 100%;
                        margin: 0 0 0 0;
                        padding: 0 0 0 0;
                        overflow: hidden;
                    }

                    iframe {
                        width: 100%;
                        height: 100%;
                    }
                </style>
            </head>
            <body>
                <iframe seamless="seamless" src="http://localhost:4567" name="${BOT_TOKEN}"></iframe>
            </body>
        </html>`
    },

    provideTextDocumentContent (uri, cancellationToken) {
        return this.htmlBotContent();
    }
}