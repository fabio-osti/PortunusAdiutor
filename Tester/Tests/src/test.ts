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
	api.setBaseAddress("http://localhost:5032/")
	const failure = (r: Response): boolean => !r.ok;
	try {
		// Step 1
		await expect.fromResponse({
			response: await api.ping(),
			logger: log.getTestAccumulator("Pings the server")
		})
		// <------------------------------|USER 1|------------------------------>
		const emailU1 = getRandomEmail("com")

		const U1 = {
			email: emailU1,
			password: "$Pass123"
		}

		// Step 2
		const tokenU1A = await expect.fromResponse({
			response: await api.signUp(U1),
			logger: log.getTestAccumulator("Creates first user")
		})

		// Step 3
		await expect.fromResponse({
			response: await api.signUp(U1),
			logger: log.getTestAccumulator("Fails to create another user with same email"),
			expect: failure
		});

		// Step 4
		await expect.fromResult({
			response: await api.whoAmI(await tokenU1A),
			logger: log.getTestAccumulator("Get claims where email-confirmed == \"False\" from token returned by step 2"),
			expect: (r) => r["email-confirmed"] === "False"
		})

		console.clear()
		const xdcCnfrCodeU1 = input(`Enter the confirmation code for ${emailU1}: `);

		// Step 5
		const cnfrCodeU1 = {
			email: emailU1,
			password: "$Pass321",
			xdc: xdcCnfrCodeU1
		}

		// Step 6
		await expect.fromResponse({
			response: await api.redefinePassword(cnfrCodeU1),
			logger: log.getTestAccumulator("Fails to redefine password with confirmation code sent at step 2"),
			expect: failure
		});

		// Step 7
		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU1),
			logger: log.getTestAccumulator("Confirms first user email with confirmation code sent at step 2")
		});

		// Step 8
		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU1),
			logger: log.getTestAccumulator("Fails to reconfirm first user email"),
			expect: failure
		});

		// Step 9
		await expect.fromResponse({
			response: await api.signIn(cnfrCodeU1),
			logger: log.getTestAccumulator("Fails to sign in with wrong password"),
			expect: failure
		})

		// Step 10
		const tokenU1B = await expect.fromResponse({
			response: await api.signIn(U1),
			logger: log.getTestAccumulator("Signs in")
		})

		// Step 11
		await expect.fromResult({
			response: await api.whoAmI(await tokenU1B),
			logger: log.getTestAccumulator("Get claims where email-confirmed == \"True\" from token returned by step 10"),
			expect: (r) => r["email-confirmed"] === "True"
		})

		// <------------------------------|USER 2|------------------------------>
		const emailU2 = getRandomEmail("adm")
		const U2 = {
			email: emailU2,
			password: "$Pass123"
		}
		// Step 12
		await expect.fromResponse({
			response: await api.signUp(U2),
			logger: log.getTestAccumulator("Creates second user with admin privileges")
		})

		// Step 13
		await expect.fromResponse({
			response: await api.sendPasswordRedefinition(U2),
			logger: log.getTestAccumulator("Sends password redefinition to the second user")
		})

		// Step 14
		await expect.fromResponse({
			response: await api.confirmEmail({
				email: emailU2,
				xdc: xdcCnfrCodeU1
			}),
			logger: log.getTestAccumulator("Fails to confirm second user email with other first user code"),
			expect: failure
		})

		const xdcRdfnCodeU2 = input(`Enter the redefinition code for ${emailU2}: `)
		const U2B = {
			email: emailU2,
			password: "Pass321$",
			xdc: xdcRdfnCodeU2
		};

		// Step 15
		await expect.fromResponse({
			response: await api.confirmEmail(U2B),
			logger: log.getTestAccumulator("Fails to confirm second user email with password redefinition code"),
			expect: failure
		})

		// Step 16
		await expect.fromResponse({
			response: await api.redefinePassword(U2B),
			logger: log.getTestAccumulator("Redefines second user password with code sent at step 15")
		})

		// Step 17
		await expect.fromResponse({
			response: await api.signIn(U2),
			logger: log.getTestAccumulator("Fails to sign second user in with old password"),
			expect: failure
		})

		// Step 18
		const tokenU2A = await expect.fromResponse({
			response: await api.signIn(U2B),
			logger: log.getTestAccumulator("Signs in")
		})

		// Step 19
		await expect.fromResult({
			response: await api.whoAmI(await tokenU2A),
			logger: log.getTestAccumulator("Get claims where email-confirmed == \"False\" from token returned by step 18"),
			expect: (r) => r["email-confirmed"] === "False"
		})

		const xdcCnfrCodeU2 = input(`Enter the confirmation code for ${emailU2}: `)
		const cnfrCodeU2 = {
			email: emailU2,
			password: "Pass321$",
			xdc: xdcCnfrCodeU2
		};

		// Step 20
		await expect.fromResponse({
			response: await api.confirmEmail(cnfrCodeU2),
			logger: log.getTestAccumulator("Confirms first user email with confirmation code sent at step 2")
		});

		// Step 21
		const tokenU2B = await expect.fromResponse({
			response: await api.signIn(U2B),
			logger: log.getTestAccumulator("Signs in")
		})

		// Step 22
		await expect.fromResponse({
			response: await api.getUsersCount(await tokenU2B),
			logger: log.getTestAccumulator("Get users count (admin only endpoint)")
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


