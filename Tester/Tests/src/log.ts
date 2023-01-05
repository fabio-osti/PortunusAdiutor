import fs from 'fs'

interface TestResult {
	title: string,
	body: any,
	success: boolean
}

const tests: TestResult[] = []

export function getTestAccumulator(title: string) {
	return function (body: any, success: boolean) {
		tests.push({
			title: title,
			body: body,
			success: success
		})
	}
}

function writeMD(filename: string) {
	const content = tests.reduce(
		(acc, cur) =>
			`${acc}` +
			`## [${cur.success ? '✔️' : '❌'}] ${cur.title}:\n` +
			`\`\`\`json\n` +
			`${JSON.stringify(cur.body, null, '\t')}\n` +
			`\`\`\`\n`,
		""
	)
	fs.writeFile(`./results/${filename}.md`, content, (err) => {
		if (err !== null) console.log(err)
	})
}

function writeTXT(filename: string) {
	const line = "-".repeat(38)
	const content = tests.reduce(
		(acc, cur) =>
			`${acc}<${line}[${cur.success ? 'V' : 'X'}]${line}>\n${cur.title}:\n${JSON.stringify(cur.body, null, '\t')}\n`,
		""
	)
	fs.writeFile(`./results/${filename}.txt`, content, (err) => {
		if (err !== null) console.log(err)
	})
	
}

function writeJSON(filename: string) {
	fs.writeFile(`./results/${filename}.json`, JSON.stringify(tests, null, '\t'), (err) => {
		if (err !== null) console.log(err)
	})
}

export function printAccumulatedTests({ writeToTxt, writeToJson, writeToMd }: { writeToTxt?: boolean; writeToJson?: boolean; writeToMd?: boolean }) {

	const filename = (new Date(Date.now())).toJSON().replaceAll('.','').replaceAll(':','').replaceAll('-','')
	
	if (writeToTxt) writeTXT(filename)
	if (writeToJson) writeJSON(filename)
	if (writeToMd) writeMD(filename)

}
