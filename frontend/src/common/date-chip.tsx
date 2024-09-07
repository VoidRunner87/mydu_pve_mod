
interface DateChipProps {
    value: Date;
}

export const DateChip = (props: DateChipProps) => {

    const date = new Date(props.value);

    return (
        <span>{date.toLocaleString()}</span>
    );
}