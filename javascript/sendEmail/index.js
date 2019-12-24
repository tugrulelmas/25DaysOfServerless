/*
 * This function is not intended to be invoked directly. Instead it will be
 * triggered by an orchestrator function.
 * 
 * Before running this sample, please:
 * - create a Durable orchestration function
 * - create a Durable HTTP starter function
 * - run 'npm install durable-functions' from the wwwroot folder of your
 *   function app in Kudu
 */
const mailgun = require('mailgun.js');

module.exports = async function (context) {
    const mg = mailgun.client({username: 'api', key: process.env["MailgunKey"]});
    const { email, title, startAt, description } = context.bindings.payload;
    const mailMessage = {
        from: process.env["FromEmail"],
        to: [email],
        subject: `Test: ${title}`,
        text: "Testing some Mailgun awesomness!",
        html: `<h4>${title} @ ${startAt}</h4> <p>${description}</p>`
      };

    mg.messages.create(process.env["FromDomain"], mailMessage);
    return msg;
};