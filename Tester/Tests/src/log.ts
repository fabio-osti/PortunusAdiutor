import fs from 'fs'

interface TestResult {
	title: string,
	body: any,
	success: boolean,
	status: number
}

const tests: TestResult[] = []
const formatOpt = { minimumIntegerDigits: 2, useGrouping: false }
const filePath = (function () {
	return __dirname
		.split('\\')
		.slice(0, -1)
		.join('\\')
		.concat(
			`\\results\\${(new Date(Date.now()))
				.toJSON()
				.replaceAll('.', '')
				.replaceAll(':', '')
				.replaceAll('-', '')}`
		)
})()

export function getTestAccumulator(title: string) {
	return function (body: any, success: boolean, status: number) {
		tests.push({
			title: title,
			body: body,
			success: success,
			status: status
		})
	}
}

function writeMD() {
	const content = tests.reduce(
		(acc, cur, i) =>
			`${acc}` +
			`## **${(i + 1).toLocaleString('en-US', formatOpt)}.** ${cur.title}: *[${cur.status}]* ${cur.success ? '✔️' : '❌'}\n` +
			`\`\`\`json\n` +
			`${JSON.stringify(cur.body, null, '\t')}\n` +
			`\`\`\`\n`,
		""
	)
	fs.writeFile(`${filePath}.md`, content, (err) => {
		if (err !== null) console.log(err)
	})
}

function writeTXT() {
	const line = "+".repeat(37)
	var divisor: string
	const content = tests.reduce(
		(acc, cur, i) =>
			acc +
			(divisor = `<${line}[${(i + 1).toLocaleString('en-US', formatOpt)}]${line}>\n`) +
			`${cur.success ? 'PASSED' : 'FAILED'} (${cur.status}): ${cur.title}\n` +
			`${"-".repeat(80).trimEnd()}\n` +
			`${JSON.stringify(cur.body, null, '\t')}\n` +
			divisor +
			"\n\n",
		""
	).trimEnd()
	fs.writeFile(`${filePath}.txt`, content, (err) => {
		if (err !== null) console.log(err)
	})

}

function writeJSON() {
	fs.writeFile(`${filePath}.json`, JSON.stringify(tests, null, '\t'), (err) => {
		if (err !== null) console.log(err)
	})
}

function writeConsole() {
	const columns = (process.stdout.columns % 2) == 0 ? process.stdout.columns : process.stdout.columns - 1
	const line = "+".repeat((columns - 6) / 2)
	tests.forEach((cur, i) => {
		const divisor = `<${line}[${(i + 1).toLocaleString('en-US', formatOpt)}]${line}>`
		console.log(divisor)
		console.log(`${cur.success ? 'PASSED' : 'FAILED'} (${cur.status}): ${cur.title}`)
		console.log("-".repeat(columns))
		console.log(cur.body)
		console.log(divisor)
		console.log("\n\n")
	});
}

export function printAccumulatedTests(
	{ writeToTxt, writeToJson, writeToMd, writeToConsole }: {
		writeToTxt?: boolean;
		writeToJson?: boolean;
		writeToMd?: boolean;
		writeToConsole?: boolean;
	}
) {
	if (writeToTxt) writeTXT()
	if (writeToJson) writeJSON()
	if (writeToMd) writeMD()
	if (writeToConsole) writeConsole()
}
