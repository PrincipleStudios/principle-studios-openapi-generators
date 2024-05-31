
export const encodeURI = (value: unknown) => encodeURIComponent(String(value));

export const throwIfNullOrUndefined = (value: unknown, nickname?: string) => {
    if (value == null) {
        throw new Error(`Parameter "${value}" was null or undefined when calling "${nickname}".`);
    }
};
