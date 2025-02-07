
interface DateChipProps {
    value: Date;
}

export const DateChip = (props: DateChipProps) => {

    const date = new Date(props.value);

    return (
        <span>{date.toLocaleString()}</span>
    );
}

interface TimeSpanChipProps
{
    value: Date;
    now: Date;
}

export const TimeSpanChip = (props: TimeSpanChipProps) => {

    const target = new Date(props.value);
    const differenceMs = target.getTime() - props.now.getTime(); // Difference in milliseconds

    const totalSeconds = Math.floor(differenceMs / 1000);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = Math.floor(totalSeconds % 60);

    let pieces = [];

    if (hours > 0)
    {
        pieces.push(`${hours}h`);
    }

    if (minutes > 0 || hours > 0)
    {
        pieces.push(`${minutes}m`)
    }

    if (minutes < 1)
    {
        pieces.push(`${seconds}s`)
    }

    const spanString = pieces.join(" ");

    return (
        <>{spanString}</>
    );
};