import prompt from 'prompt-sync';
const input = prompt()

import * as api from './api'
import * as expect from './expect'
import * as log from './log'

function getRandomEmail() {
	const usr = (Math.random() + 1).toString(36).substring(2);
	const prv = (Math.random() + 1).toString(36).substring(2);
	return `${usr}@${prv}.com`;
}

(async function () {
	api.setBaseAddress("http://localhost:5032/")
	const failure = (r: Response): boolean => !r.ok;
	try {


		await expect.fromResponse({
			response: await api.ping(),
			logger: log.getTestAccumulator("Ping")
		})
		// <------------------------------|USER 1|------------------------------>
		const emailU1 = getRandomEmail()

		const U1 = {
			email: emailU1,
			password: "$Pass123"
		}

		const tokenU1A = await expect.fromResponse({
			response: await api.signUp(U1),
			logger: log.getTestAccumulator("First User")
		})

		await expect.fromResponse({
			response: await api.signUp(U1),
			logger: log.getTestAccumulator("Failed second attempt"),
			expect: failure
		});

		await expect.fromResult({
			response: await api.whoAmI(await tokenU1A),
			logger: log.getTestAccumulator("Claims"),
			expect: (r) => r["email-confirmed"] === "False"
		})

		const xdcCnfrCodeU1 = input(`Enter the confirmation code for ${emailU1}: `);
		const cnfrCodeU1 = {
			email: emailU1,
			password: "$Pass321",
			xdc: xdcCnfrCodeU1
		}

		await expect.fromResponse({
			response: await api.redefinePassword(cnfrCodeU1),
			logger: log.getTestAccumulator("Failed password redefinition"),
			expect: failure
		});

		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU1),
			logger: log.getTestAccumulator("Email confirmation")
		});

		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU1),
			logger: log.getTestAccumulator("Failed email reconfirmation"),
			expect: failure
		});

		await expect.fromResponse({
			response: await api.signIn(cnfrCodeU1),
			logger: log.getTestAccumulator("Failed sign in with wrong password"),
			expect: failure
		})

		const tokenU1B = await expect.fromResponse({
			response: await api.signIn(U1),
			logger: log.getTestAccumulator("Sign in")
		})

		await expect.fromResult({
			response: await api.whoAmI(await tokenU1B),
			logger: log.getTestAccumulator("Claims"),
			expect: (r) => r["email-confirmed"] === "True"
		})

		// <------------------------------|USER 2|------------------------------>
		const emailU2 = getRandomEmail()
		const U2 = {
			email: emailU2,
			password: "$Pass123"
		}
		await expect.fromResponse({
			response: await api.signUp(U2),
			logger: log.getTestAccumulator("Second User")
		})

		await expect.fromResponse({
			response: await api.sendPasswordRedefinition(U2),
			logger: log.getTestAccumulator("Sent password redefinition")
		})

		await expect.fromResponse({
			response: await api.confirmEmail({
				email: emailU2,
				xdc: xdcCnfrCodeU1
			}),
			logger: log.getTestAccumulator("Failed email confirmation with other user code"),
			expect: failure
		})

		const xdcRdfnCodeU2 = input(`Enter the redefinition code for ${emailU2}: `)
		const U2B = {
			email: emailU2,
			password: "Pass321$",
			xdc: xdcRdfnCodeU2
		};

		await expect.fromResponse({
			response: await api.confirmEmail(U2B),
			logger: log.getTestAccumulator("Failed email confirmation with password redefinition code"),
			expect: failure
		})

		await expect.fromResponse({
			response: await api.redefinePassword(U2B),
			logger: log.getTestAccumulator("Password redefined")
		})

		await expect.fromResponse({
			response: await api.signIn(U2),
			logger: log.getTestAccumulator("Failed sign in with old password"),
			expect: failure
		})

		const tokenU2A = await expect.fromResponse({
			response: await api.signIn(U2B),
			logger: log.getTestAccumulator("Sign in")
		})

		await expect.fromResult({
			response: await api.whoAmI(await tokenU2A),
			logger: log.getTestAccumulator("Claims"),
			expect: (r) => r["email-confirmed"] === "False"
		})
	} catch (e) {
	} finally {
		log.printAccumulatedTests({
			writeToTxt: true,
			writeToJson: true,
			writeToMd: true
		})
	}
})()


