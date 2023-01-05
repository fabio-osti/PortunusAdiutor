async function tryGetJson(response: Response) {
	try {
		return JSON.parse(await response.text())
	} catch (e) {
		return {}
	}
}

export async function fromResult({ response, expect, logger }: {
	response: Response;
	expect: (result: any) => boolean
	logger?: (result: any, success: boolean) => void
}) {
	const result = await tryGetJson(response)

	if (!expect(result)) {
		logger?.(result, false)
		throw Error(`${response.statusText} ${JSON.stringify(result)}`);
	}

	logger?.(result, true)

	return result
}

export async function fromResponse({ response, expect, logger }: {
	response: Response;
	expect?: (response: Response) => boolean
	logger?: (result: any, success: boolean) => void
}) {
	expect = expect ?? ((r) => r.ok)
	if (!expect(response)) {
		const result = await tryGetJson(response)
		logger?.(result, false)
		throw Error(`${response.statusText} ${result}`);
	}

	const result = await tryGetJson(response)
	logger?.(result, true)

	return result
}