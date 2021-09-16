
export const encodeURI = (value: any) => encodeURIComponent(String(value));

export const throwIfNullOrUndefined = (value: any, nickname?: string) => {
    if (value == null) {
        throw new Error(`Parameter "${value}" was null or undefined when calling "${nickname}".`);
    }
};
