import prompt from 'prompt-sync';
const input = prompt()

import * as api from './api'
import * as expect from './expect'
import * as log from './log'

function getRandomEmail(domainExt: string) {
	const usr = (Math.random() + 1).toString(36).substring(2);
	const prv = (Math.random() + 1).toString(36).substring(2);
	return `${usr}@${prv}.${domainExt}`;
}

(async function () {
	console.clear()
	api.setBaseAddress("http://localhost:8080/")
	const failure = (r: Response): boolean => !r.ok;
	try {
		await expect.fromResponse({
			response: await api.ping(),
			logger: log.getTestAccumulator("Should ping the server")
		})
		// <------------------------------|USER 1|------------------------------>
		const emailU1 = getRandomEmail("com")

		const U1 = {
			email: emailU1,
			password: "$Pass123"
		}

		const tokenU1A = await expect.fromResponse({
			response: await api.signUp(U1),
			logger: log.getTestAccumulator("Should create first user")
		})

		await expect.fromResponse({
			response: await api.signUp(U1),
			logger: log.getTestAccumulator("Should fail to create another user with same email"),
			expect: failure
		});

		await expect.fromResult({
			response: await api.whoAmI(await tokenU1A),
			logger: log.getTestAccumulator("Should get claims where email-confirmed == \"False\" and is-admin == \"False\" from token returned by sign up"),
			expect: (r) => r["email-confirmed"] === "False" && r["is-admin"] === "False"
		})

		const ConfirmationXdcU1 = input(`Enter the confirmation code for ${emailU1}: `);
		const cnfrCodeU1 = {
			email: emailU1,
			password: "$Pass321",
			xdc: ConfirmationXdcU1
		}

		await expect.fromResponse({
			response: await api.redefinePassword(cnfrCodeU1),
			logger: log.getTestAccumulator("Should fail to redefine password with confirmation code sent at sign up"),
			expect: failure
		});

		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU1),
			logger: log.getTestAccumulator("Should confirm first user email with confirmation code sent at sign up")
		});

		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU1),
			logger: log.getTestAccumulator("Should fail to reconfirm first user email"),
			expect: failure
		});

		await expect.fromResponse({
			response: await api.signIn(cnfrCodeU1),
			logger: log.getTestAccumulator("Should fail to sign in with wrong password"),
			expect: failure
		})

		const tokenU1B = await expect.fromResponse({
			response: await api.signIn(U1),
			logger: log.getTestAccumulator("Should sign in")
		})

		await expect.fromResult({
			response: await api.whoAmI(await tokenU1B),
			logger: log.getTestAccumulator("Should get claims where email-confirmed == \"True\" from token returned by sign in"),
			expect: (r) => r["email-confirmed"] === "True"
		})

		// <------------------------------|USER 2|------------------------------>
		const emailU2 = getRandomEmail("adm")
		const U2 = {
			email: emailU2,
			password: "$Pass123"
		}

		await expect.fromResponse({
			response: await api.signUp(U2),
			logger: log.getTestAccumulator("Should create second user with admin privileges")
		})

		await expect.fromResponse({
			response: await api.sendPasswordRedefinition(U2),
			logger: log.getTestAccumulator("Should send password redefinition to the second user")
		})

		await expect.fromResponse({
			response: await api.confirmEmail({
				email: emailU2,
				xdc: ConfirmationXdcU1
			}),
			logger: log.getTestAccumulator("Should fail to confirm second user email with other first user code"),
			expect: failure
		})

		const RedefinitionXdcU2 = input(`Enter the redefinition code for ${emailU2}: `)
		const U2B = {
			email: emailU2,
			password: "Pass321$",
			xdc: RedefinitionXdcU2
		};

		await expect.fromResponse({
			response: await api.confirmEmail(U2B),
			logger: log.getTestAccumulator("Should fail to confirm second user email with password redefinition code"),
			expect: failure
		})

		await expect.fromResponse({
			response: await api.redefinePassword(U2B),
			logger: log.getTestAccumulator("Should redefine second user password with code sent")
		})

		// Makes sure the authentication endpoint doesn't authenticate anyone and that the password was really changed at redefinition
		await expect.fromResponse({
			response: await api.signIn(U2),
			logger: log.getTestAccumulator("Should fail to sign second user in with old password"),
			expect: failure
		})

		const tokenU2A = await expect.fromResponse({
			response: await api.signIn(U2B),
			logger: log.getTestAccumulator("Should sign in")
		})

		// Makes sure the email wasn't confirmed by the (supposedly) attempts and that redefining the password doesn't cause the email to be confirmed
		await expect.fromResult({
			response: await api.whoAmI(await tokenU2A),
			logger: log.getTestAccumulator("Should get claims where email-confirmed == \"False\" is-admin == \"True\" from token returned by sign in"),
			expect: (r) => r["email-confirmed"] === "False" && r["is-admin"] === "True"
		})

		await expect.fromResponse({
			response: await api.sendEmailConfirmation(U2),
			logger: log.getTestAccumulator("Should send email confirmation code to the second user")
		})

		const xdcCnfrCodeU2 = input(`Enter the second confirmation code sent for ${emailU2}: `)
		const cnfrCodeU2 = {
			email: emailU2,
			password: "Pass321$",
			xdc: xdcCnfrCodeU2
		};

		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU2),
			logger: log.getTestAccumulator("Should confirm second user email with confirmation code sent at step 19")
		});

		const tokenU2B = await expect.fromResponse({
			response: await api.signIn(U2B),
			logger: log.getTestAccumulator("Should sign in")
		})

		await expect.fromResponse({
			response: await api.getUsersCount(await tokenU2B),
			logger: log.getTestAccumulator("Should get users count (admin only endpoint)")
		})

		await expect.fromResponse({
			response: await api.getUsersCount(await tokenU1B),
			logger: log.getTestAccumulator("Should fail to get users count (admin only endpoint)"),
			expect: r => r.status === 403
		})
	} catch (e) {
	} finally {
		log.printAccumulatedTests({
			writeToTxt: true,
			writeToJson: true,
			writeToMd: true,
			writeToConsole: true
		})
	}
})()


