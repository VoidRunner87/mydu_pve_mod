import styled from 'styled-components'

const Container = styled.div`
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    z-index: 99999;
`;

const Panel = styled.div`
    width: 55vw;
    height: 75vh;
    background-color: rgb(25,34,41);
    border: 1px solid rgb(50,79,77);
    padding: 2px;
    justify-content: center;
    align-items: center;
`;

const Header = styled.div`
    background-color: rgb(0,0,0);
    padding: 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
`;

const Title = styled.span`
    color: rgb(255, 255, 255);
    font-size: 18px;
    margin-right: 10px;
`;

const CharButton = styled.button`
    margin-left: auto;
    background-color: transparent;
    border: none;
    font-size: 40px;
    line-height: 0;
    cursor: pointer;
    padding: 0;
    color: rgb(255, 255, 255);
`

const QuestList = (props) => {
    return (
        <Container>
            <Panel>
                <Header>
                    <Title>Red Talon Enclave Jobs</Title>
                    <CharButton>&times;</CharButton>
                </Header>
            </Panel>
        </Container>
    );
}

export default QuestList;