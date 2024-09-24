import {CloseButton, Container, Header, Panel, Title} from "./panel";

const QuestList = (props) => {
    return (
        <Container>
            <Panel>
                <Header>
                    <Title>Red Talon Enclave Jobs</Title>
                    <CloseButton>&times;</CloseButton>
                </Header>
            </Panel>
        </Container>
    );
}

export default QuestList;